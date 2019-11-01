const config = {
    app: {
      port: 8080,
      uploads: __dirname + '/uploads/'
    },
    db: {
      uri: 'mongodb+srv://crowbar:crowbar@crowbar-xrbmq.mongodb.net/test?retryWrites=true&w=majority'
    }
   };
   
   module.exports = config;