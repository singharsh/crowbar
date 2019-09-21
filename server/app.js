const express = require('express');
const bodyParser = require('body-parser');
const status = require('http-status-codes');
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
            res.status(status.INTERNAL_SERVER_ERROR).send();
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
            res.status(status.INTERNAL_SERVER_ERROR).send();
        }
    });

    // create user || update user credentials
    app.post('/users', async (req, res) => {
        if ('username' in req.query && 'password' in req.query) {
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
            } else { // create new user account
                if ('email' in req.query) {
                    if (await users.create(req.query.username, req.query.password, req.query.email)) {
                        res.status(status.OK).send(await users.get(req.query.username));
                    } else {
                        res.status(status.INTERNAL_SERVER_ERROR).send();
                    }
                } else {
                    res.status(status.INTERNAL_SERVER_ERROR).send();
                }
            }
        } else {
            res.status(status.INTERNAL_SERVER_ERROR).send();
        }
    });

    // get repo details
    app.get('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query) {
            if (await repos.exists(req.query.repo)) { // repo exists
                if (! await users.authenticate(req.puery.username, req.query.password) || ! await repos.hasAccess(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                res.status(status.OK).send(await repos.get(erq.query.repo));
            } else {
                res.status(status.NOT_FOUND).send();
            }
        } else {
            res.status(status.INTERNAL_SERVER_ERROR).send();
        }
    });

    // create repo || update repo
    app.post('/repos', async (req, res) => {
        if ('repo' in req.query && 'username' in req.query && 'password' in req.query) {
            if (await repos.exists(req.query.repo)) { // update details if repo exists
                if (! await users.authenticate(req.puery.username, req.query.password) || ! await repos.isOwner(req.query.repo, req.query.username)) { // authenticate user and check if user has access
                    res.status(status.UNAUTHORIZED).send();
                }
                if ('owner' in req.query) { // add owner
                    await repos.addOwner(req.query.repo, req.query.username);
                }
                if ('collaborator' in req.query) { // add collaborator
                    await repos.addCollaborator(req.query.repo, req.query.collaborator);
                }
                res.status(status.OK).send(await repos.get(req.query.repo));
            } else { // create a new repo
                if (await repos.create(req.query.repo)) {
                    res.status(status.OK).send(await repos.get(req.query.repo));
                } else {
                    res.status(status.INTERNAL_SERVER_ERROR).send();
                }
            }
        } else {
            res.status(status.INTERNAL_SERVER_ERROR).send();
        }
    });
});

module.exports = { app };