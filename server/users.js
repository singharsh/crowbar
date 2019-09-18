function authenticate(userName, password) {
    return false;
}

function exists(userName) {
    return false;
}

function create(userName, password, email) {
    return -1;
}

module.exports = { authenticate, create }