const express = require('express');
const bodyParser = require('body-parser');
const config = require('./config');
const users = require('./users');
const repos = require('./repos');
const status = require('http-status-codes');

// create express app
const app = express();

// parse requests of content-type - application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: true }))

// parse requests of content-type - application/json
app.use(bodyParser.json())

// define a simple route
app.get('/', (req, res) => {
    res.send("crowbar API");
});

// authenticate user
app.post('/authenticate', (req, res) => {
    if ('username' in req.query && 'password' in req.query) {
        if (users.authenticate(req.query.username, req.query.password)) {
            res.status(status.OK).send();
        } else {
            res.status(status.UNAUTHORIZED).send();
        }
    } else {
        res.status(status.INTERNAL_SERVER_ERROR).send();
    }
});

// create user
app.get('/user', (req, res) => {
    if ('username' in req.query && 'password' in req.query && 'email' in req.query) {
        if (id = users.create(req.query.username, req.query.password, req.query.email) > 0) {
            let response = {
                id: 101
            }
            res.status(status.OK).send(response);
        } else {
            
        }
    } else {
        res.status(status.INTERNAL_SERVER_ERROR).send();
    }
});

// update user credentails
app.post('/user', (req, res) => {
    res.send("");
});

// listen for requests
app.listen(config.app.port, () => {
    console.log("server is listening on port " + config.app.port);
});

module.exports = app;