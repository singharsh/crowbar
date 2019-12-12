#! /bin/bash

# BlackBox Testing Script for Client

# create a new user account
crowbar signup --user-name test_user --email testuser@mail.com --password Harsh!2345

if [ $? -ne 0 ]
then
	echo "couldn't create a new user"
	exit -1
fi

echo "created a new account"

# login to user account
rm -f ~/crowbar.json

crowbar login --user-name test_user --password Harsh!2345

if [ $? -ne 0 ]
then
	echo "couldn't login"
	exit -1
fi

echo "logged in"

# update email & password
crowbar credentials --email test@mail.com --password Harsh!23456

if [ $? -ne 0 ]
then
	echo "couldn't update credentials"
	exit -1
fi

rm -f ~/crowbar.json

crowbar login --user-name test_user --password Harsh!23456

if [ $? -ne 0 ]
then
	echo "couldn't login"
	exit -1
fi

echo "updated credentails"

# create a new repo
crowbar new --repo test_repo

if [ $? -ne 0 ]
then
	echo "couldn't create a new repo"
	exit -1
fi

echo "created repo"

# make a commit
pushd test_repo

echo "hi there! testing..." > somefile
cp somefile ..

crowbar push --message "my first commit"

if [ $? -ne 0 ]
then
	echo "couldn't commit"
	exit -1
fi

echo "pushed to repo"

popd

rm -rf test_repo

# clone a repo
crowbar clone --repo test_repo

if [ $? -ne 0 ]
then
	echo "couldn't clone repo"
	exit -1
fi

echo "pulled from repo"

pushd test_repo

diff -Bb somefile ../somefile

if [ $? -ne 0 ]
then
	echo "files aren't the same"
	exit -1
fi

popd

echo "tests passed!"

exit 0
