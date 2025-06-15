
add loginnname in tblusers

rm is used for the access level, admin, main and local, all lowercase

Copy all the usernames to login names
Login name will be used to log in from now onwards, 
new registers will use bcrypt.
UPDATE tblusers
SET loginname = COALESCE(username, loginname);