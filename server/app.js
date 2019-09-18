const express = require('express');
const bodyParser = require('body-parser');
const config = require('./config');

// create express app
const app = express();

// parse requests of content-type - application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: true }))

// parse requests of content-type - application/json
app.use(bodyParser.json())

// define a simple route
app.get('/', (req, res) => {
    res.json({"message": "crowbar API"});
});

// listen for requests
app.listen(config.app.port, () => {
    console.log("server is listening on port " + config.app.port);
});