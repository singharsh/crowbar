const express = require('express');
const bodyParser = require('body-parser');
const status = require('http-status-codes');
const multer  =   require('multer');
const mkdir = require('mkdirp');
const fs = require("fs");
const db = require('./db');
const config = require('./config');
const users = require('./users');
const repos = require('./repos');

// create express app
const app = express();

// parse requests of content-type - application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: true }))

// parse requests of content-type - application/json
app.use(bodyParser.json())

const storage =   multer.diskStorage({
    destination: function (req, file, callback) {
        const dir = config.app.uploads + req.query.repo;
        mkdir(dir, err => callback(err, dir));
    },
    filename: function (req, file, callback) {
      callback(null, file.fieldname + '-' + req.query.timestamp + '.zip');
    }
});
const upload = multer({ storage : storage}).single('commitZIP');

db.connect( async (error) => {
    if (error) {
        console.log(error);
    }
    console.log("connected to mongodb");
    // listen for requests
    let port = process.env.PORT || config.app.port;
    app.listen(port, () => {
        console.log("server is listening on port " + port);
    });

    // define a simple route
    app.get('/', async (req, res) => {
        res.send("crowbar API");
    });

    // authenticate user
    app.post('/authenticate', async (req, res) => {
        if ('username' in req.query && 'password' in req.query) {
            if (await users.authenticate(req.query.username, req.query.password)) {
                res.status(status.OK).send();
            } else {
                res.status(status.UNAUTHORIZED).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // get user details
    app.get('/users', async (req, res) => {
        if ('username' in req.query) {
            if (await users.exists(req.query.username)) { // user exists
                res.status(status.OK).send(await users.get(req.query.username));    
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // create user
    app.post('/users', async (req, res) => {
        if ('username' in req.query && 'password' in req.query && 'email' in req.query) {
            if (await users.exists(req.query.username)) { // user exists already
                res.status(status.NOT_FOUND).send();
            } else { // create new user account
                if (await users.create(req.query.username, req.query.password, req.query.email)) {
                    res.status(status.OK).send(await users.get(req.query.username));
                } else {
                    res.status(status.INTERNAL_SERVER_ERROR).send();
                }
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // update user credentials
    app.put('/users', async (req, res) => {
        if ('username' in req.query && 'password' in req.query && ('email' in req.query || 'updatedpassword' in req.query)) {
            if (await users.exists(req.query.username)) { // update credentials if user exists
                if (! await users.authenticate(req.query.username, req.query.password)) { // authenticate user
                    res.status(status.UNAUTHORIZED).send();
                }
                if ('email' in req.query) { // update email
                    await users.updateEmail(req.query.username, req.query.email);
                }
                if ('updatedpassword' in req.query) { // update password
                    await users.updatePassword(req.query.username, req.query.updatedpassword);
                }
                res.status(status.OK).send(await users.get(req.query.username));
            } else { // user doesn't exist
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // remove user
    app.delete('/users', async (req, res) => {
        if ('username' in req.query && 'password' in req.query) {
            if (await users.exists(req.query.username)) {
                if (! await users.authenticate(req.query.username, req.query.password)) { // authenticate user
                    res.status(status.UNAUTHORIZED).send();
                }
                await users.remove(req.query.username);
                res.status(status.OK).send();
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // get repo details
    app.get('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query) {
            if (await repos.exists(req.query.repo)) { // repo exists
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.hasAccess(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                res.status(status.OK).send(await repos.get(req.query.repo));
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // create repo
    app.post('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query) {
            if (await repos.exists(req.query.repo) || !await users.exists(req.query.username)) { // repo exists already or user doesn't exist
                res.status(status.NOT_FOUND).send();
            } else { // create a new repo
                if (await repos.create(req.query.repo)) {
                    await repos.addOwner(req.query.repo, req.query.username);
                    res.status(status.OK).send(await repos.get(req.query.repo));
                } else {
                    res.status(status.INTERNAL_SERVER_ERROR).send();
                }
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // update repo
    app.put('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query && ('owner' in req.query || 'collaborator' in req.query)) {
            if (await repos.exists(req.query.repo)) { // update details if repo exists
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.isOwner(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                if ('owner' in req.query) { // add owner
                    if (!await users.exists(req.query.owner)) {
                        res.status(status.NOT_FOUND).send(await repos.get(req.query.repo));
                    }
                    await repos.addOwner(req.query.repo, req.query.owner);
                }
                if ('collaborator' in req.query) { // add collaborator
                    if (!await users.exists(req.query.collaborator)) {
                        res.status(status.NOT_FOUND).send(await repos.get(req.query.repo));
                    }
                    await repos.addCollaborator(req.query.repo, req.query.collaborator);
                }
                res.status(status.OK).send(await repos.get(req.query.repo));
            } else { // repo doesn't exist
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // remove repo || remove owner or collaborator
    app.delete('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query) {
            if (await repos.exists(req.query.repo)) { // update details if repo exists
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.isOwner(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                if ('owner' in req.query || 'collaborator' in req.query) {
                    if ('owner' in req.query) { // remove owner
                        await repos.removeOwner(req.query.repo, req.query.username);
                    }
                    if ('collaborator' in req.query) { // remove collaborator
                        await repos.removeCollaborator(req.query.repo, req.query.collaborator);
                    }
                    res.status(status.OK).send(await repos.get(req.query.repo));
                } else {
                    await repos.remove(req.query.repo); // delete repo
                    res.status(status.OK).send();
                }
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // upload commit
    app.post('/commits', async (req, res) => {
        if ('message' in req.query && 'repo' in req.query && 'username' in req.query && 'password' in req.query) {
            if (await repos.exists(req.query.repo)) {
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.hasAccess(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                const timestamp = Date.now();
                req.query.timestamp = timestamp;
                upload(req, res, async function(err) {
                    if (err) {
                        res.status(status.INTERNAL_SERVER_ERROR).send();
                    } else {
                        res.status(status.OK).send(await repos.pushCommit(req.query.repo, req.query.username, req.query.message, timestamp));
                    }
                });
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // download commit
    app.get('/commits', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query && 'id' in req.query) {
            if (await repos.exists(req.query.repo)) {
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.hasAccess(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                if (repos.pullCommit(req.query.repo, req.query.id) != null) {
                    const path = config.app.uploads + req.query.repo + '/commitZIP-' + req.query.id + '.zip';
                    if (fs.existsSync(path)) {
                        res.download(path);
                    } else {
                        res.status(status.INTERNAL_SERVER_ERROR).send();
                    }
                } else {
                    res.status(status.NOT_FOUND).send();
                }
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });

    // delete commit
    app.delete('/commits', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query && 'id' in req.query) {
            if (await repos.exists(req.query.repo)) {
                if (! await users.authenticate(req.query.username, req.query.password) || ! await repos.isOwner(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                await repos.removeCommit(req.query.repo, req.query.username, req.query.id); // delete commit
                const path = config.app.uploads + req.query.repo + '/commitZIP-' + req.query.id + '.zip';
                if (fs.existsSync(path)) {
                    fs.unlinkSync(path);
                }
                res.status(status.OK).send();
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.NOT_ACCEPTABLE).send();
        }
    });
});

module.exports = { app };