const util = require('./db');

async function get(user) {
    // return user name & email
    const db = util.get();
    let items = await db.collection('users').find({"username": user}, {}).toArray();
    for (i = 0; i < items.length; i++) {
        const result = {
            "username": items[i].username,
            "email": items[i].email
        };
        return result;
    }
    return null;
}

async function exists(user) {
    // return true if the user exists, false otherwise
    const db = util.get();
    let items = await db.collection('users').find({"username": user}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        if (!items[i].deleted) {
            return true;
        }
    }
    return false;
}

async function authenticate(user, password) {
    // return true if the user name and password match, false otherwise
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
    // create a user with the given crdentials and return true, false if the user already exists
    if (await exists(user)) {
        return false;
    }
    const db = util.get();
    await db.collection('users').remove({"username": user}, {});
    await db.collection('users').insertOne({
        "username": user,
        "password": password,
        "email": email,
        "deleted": false
    });
    return true;
}

async function remove(user) {
    // remove user if exists and return true, false otherwise
    if (!await exists(user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('users').updateOne({"username": user}, {"$set": {"deleted": true}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function updatePassword(user, password) {
    // update password of the given user if the user exists and return true, false otherwise
    if (!await exists(user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('users').updateOne({"username": user}, {"$set": {"password": password}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function updateEmail(user, email) {
    // update email of the given user if the user exists and return true, false otherwise
    if (!await exists(user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('users').updateOne({"username": user}, {"$set": {"email": email}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

module.exports = { get, exists, authenticate, create, remove, updatePassword, updateEmail };