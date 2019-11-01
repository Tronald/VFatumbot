'use strict';
const todoList = require('../controllers/Controller');
 
module.exports = function(app) {
 
  //GET route for version
  app.route('/Version')
  	.get(todoList.list_version);
  
  //GET route for sizes
  app.route('/sizes')
    .get(todoList.sizes);

  //GET for the running the pseudo instance
  app.route('/pseudo')
    .get(todoList.psuedo);

  //POST for the setting own entropy
  app.route('/setentropy')
    .post(todoList.setentropy);

  //GET for getting entropy
  app.route('/entropy')
    .get(todoList.entropy);

  //GET for getting pool
  app.route('/pool')
    .get(todoList.getPool);

  //GET for getting attractors
  app.route('/attractors')
    .get(todoList.attractors);

};

  