const tingoDB = require('tingodb')().Db;
const config = require('./config');

// database connection
const db = new tingoDB(config.db.path, {});

const get = async function (user) {
    // TODO: return user name & email
    return await db.collection('users').findOne({username: user}, 
        function(error, item) {
            if (error) {
                console.log(error);
            }
            if (item) {
                const result = {
                    "username": item.username,
                    "email": item.email
                };
                console.log(result)
                return result;
            }
            return {}; 
        });
}

function exists(user) {
    // TODO: return true if the user exists, false otherwise
    let item = db.collection('users').find({"username": user});
    if (item) {
        return true;
    } else {
        false;
    }
}

function authenticate(user, password) {
    // TODO: return true if the user name and password match, false otherwise
    let item = db.collection('users').find({"username": user});
    if (item) {
        return item.password == password;
    } else {
        return false;
    }
}

function create(user, password, email) {
    // TODO: create a user with the given crdentials and return true, false if the user already exists
    if (exists(user)) {
        return false;
    }
    db.collection('users').insert({
        "username": user,
        "password": password,
        "email": email
    });
    return true;
}

function updatePassword(user, password) {
    // TODO: update password of the given user if the user exists and return true, false otherwise
    return false;
}

function updateEmail(user, email) {
    // TODO: update email of the given user if the user exists and return true, false otherwise
    return false;
}

module.exports = { get, exists, authenticate, create, updatePassword, updateEmail };