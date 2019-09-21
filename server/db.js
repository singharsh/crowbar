const MongoClient = require( 'mongodb' ).MongoClient;
const config = require('./config');
const uri = config.db.uri;

let _db;

const connect = async (callback) => {
    try {
        MongoClient.connect(uri, (error, client) => {
            _db = client.db('crowbar');
            return callback(error);
        })
    } catch (e) {
        throw e;
    }
}

const get = () => _db;

const disconnect = () => _db.close();

module.exports = { connect, get, disconnect };