const util = require('./db');

async function get(user) {
    // TODO: return user name & email
    const db = util.get();
    let items = await db.collection('users').find({"username": user}, {}).toArray();
    for (i = 0; i < items.length; i++) {
        const result = {
            "username": items[i].username,
            "email": items[i].email
        };
        return result;
    }
    return {};
}

async function exists(user) {
    // TODO: return true if the user exists, false otherwise
    const db = util.get();
    let items = await db.collection('users').find({"username": user}, {}).toArray().then(items => {
        return items;
    });
    return items.length > 0;
}

async function authenticate(user, password) {
    // TODO: return true if the user name and password match, false otherwise
    const db = util.get();
    let items = await db.collection('users').find({"username": user}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        return items[i].password == password;
    }
    return false;
}

async function create(user, password, email) {
    // TODO: create a user with the given crdentials and return true, false if the user already exists
    if (exists(user)) {
        return false;
    }
    const db = util.get();
    await db.collection('users').insertOne({
        "username": user,
        "password": password,
        "email": email
    });
    return true;
}

async function updatePassword(user, password) {
    // TODO: update password of the given user if the user exists and return true, false otherwise
    return false;
}

async function updateEmail(user, email) {
    // TODO: update email of the given user if the user exists and return true, false otherwise
    return false;
}

module.exports = { get, exists, authenticate, create, updatePassword, updateEmail };