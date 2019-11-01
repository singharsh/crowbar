const util = require('./db');
const date = require('date-and-time');

async function get(repo) {
    // return repo name, owners & collaborators
    const db = util.get();
    let items = await db.collection('repos').find({"repo": repo}, {}).toArray();
    for (i = 0; i < items.length; i++) {
        const result = {
            "repo": items[i].repo,
            "owners": items[i].owners,
            "collaborators": items[i].collaborators,
            "commits": items[i].commits
        };
        return result;
    }
    return null;
}

async function exists(repo) {
    // return true if the repo exists, false otherwise
    const db = util.get();
    let items = await db.collection('repos').find({"repo": repo}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        if (!items[i].deleted) {
            return true;
        }
    }
    return false;
}

async function create(repo) {
    // create a repo and return true, false if repo already exists
    if (await exists(repo)) {
        return false;
    }
    const db = util.get();
    await db.collection('repos').remove({"repo": repo}, {});
    await db.collection('repos').insertOne({
        "repo": repo,
        "owners": [],
        "collaborators": [],
        "commits": [],
        "deleted": false
    });
    return true;
}

async function remove(repo) {
    // delete a repo if it exists and return true, false otherwise
    if (!await exists(repo)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$set": {"deleted": true}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function addOwner(repo, user) {
    // add user as owner of the repo if repo exists and return true, false otherwise
    if (!await exists(repo) || await hasAccess(repo, user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$push": {"owners": user}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function addCollaborator(repo, user) {
    // add user as collaborator of the repo if repo exists and return true, false otherwise
    if (!await exists(repo) || await hasAccess(repo, user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$push": {"collaborators": user}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function removeOwner(repo, user) {
    // remove user as owner of the repo if repo exists and user was an owner and return true, false otherwise
    if (!await exists(repo) || !await isOwner(repo, user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$pull": {"owners": user}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function removeCollaborator(repo, user) {
    // remove user as collaborator of the repo if repo exists and user was a collaborator and return true, false otherwise
    if (!await exists(repo) || !await isCollaborator(repo, user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$pull": {"collaborators": user}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        }
    });
}

async function isOwner(repo, user) {
    // return true if the user is owner of the given repo, false otherwise
    const db = util.get();
    let items = await db.collection('repos').find({"repo": repo}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        if (items[i].owners.includes(user)) {
            return true;
        }
    }
    return false;
}

async function isCollaborator(repo, user) {
    // return true if the user is collaborator of the given repo, false otherwise
    const db = util.get();
    let items = await db.collection('repos').find({"repo": repo}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        if (items[i].collaborators.includes(user)) {
            return true;
        }
    }
    return false;
}

async function hasAccess(repo, user) {
    return isOwner(repo, user) || isCollaborator(repo, user);
}

async function pushCommit(repo, user, message, id) {
    // record commit to the repo if the repo exists and the user has access
    if (!await exists(repo) || !await hasAccess(repo, user)) {
        return null;
    }
    const now = date.format(new Date(), 'MMM. D YYYY - HH:mm:ss [GMT]Z');
    const commit = {
        "id": id,
        "user": user,
        "message": message,
        "timestamp": now
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$push": {"commits": commit}}, {}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            commit.repo = repo;
            return commit;
        }
    });
}

async function pullCommit(repo, id) {
    // return the commit # if the repo and commit exist, null otherwise
    const db = util.get();
    let items = await db.collection('repos').find({"repo": repo}, {}).toArray().then(items => {
        return items;
    });
    for (i = 0; i < items.length; i++) {
        for (j = 0; j < items[i].commits.length; j++) {
            if (items[i].commits[j].id == id) {
                return id;
            }
        }
    }
    return null;
}

async function removeCommit(repo, user, id) {
    // FIXME: remove the commit if the commit exists and the user has access to the repo
    if (!await exists(repo) || !await isOwner(repo, user)) {
        return false;
    }
    const db = util.get();
    return await db.collection('repos').updateOne({"repo": repo}, {"$pull": {"commits": {"id": id}}}, {multi: true}).then(result => {
        const { matchedCount, modifiedCount } = result;
        if (matchedCount && modifiedCount) {
            return true;
        } else {
            return false;
        }
    });
}

module.exports = { get, exists, create, remove, addOwner, addCollaborator, removeOwner, removeCollaborator, isOwner, isCollaborator, hasAccess, pushCommit, pullCommit, removeCommit };