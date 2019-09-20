function get(repo) {
    // TODO: return repo name, owners & collaborators
    return null;
}

function exists(repo) {
    // TODO: return true if the repo exists, false otherwise
    return false;
}

function create(repo) {
    // TODO: create a repo and return true, false if repo already exists
    return -1;
}

function addOwner(repo, user) {
    // TODO: add user as owner of the repo if repo exists and return true, false otherwise
    return false;
}

function addCollaborator(repo, user) {
    // TODO: add user as collaborator of the repo if repo exists and return true, false otherwise
    return false;
}

function isOwner(repo, user) {
    // TODO: return true if the user is owner of the given repo, false otherwise
    return false;
}

function isCollaborator(repo, user) {
    // TODO: return true if the user is collaborator of the given repo, false otherwise
    return false;
}

function hasAccess(repo, user) {
    return isOwner(repo, user) || isCollaborator(repo, user);
}

module.exports = { get, exists, create, addOwner, addCollaborator, isOwner, isCollaborator, hasAccess };