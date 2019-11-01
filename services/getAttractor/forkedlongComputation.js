const qrng = require("../anuapi/anuapi.js");
const addon = require('../../build/Release/AttractFunctions');
const cont = require('../../controllers/Controller');
var now = require("performance-now")
const isValidCoordinates = require('is-valid-coordinates');

const longComputation = (limit, callback) => {
  
  //Set variables recieved from the main function.
  var GID = limit.GID;
  var x = limit.x;
  var y = limit.y;
  var radius = limit.radius;
  var filter = limit.filter;
  var handle = limit.handle;
  
  //Check coordinates
  var centervalid = isValidCoordinates(y, x)


  //Check if GID is found
  qrng.getentropy(GID, function(result) {
  	   if (result == "1") {callback("GID invalid");
       } else if (centervalid == false) {callback("Coordinates are invalid") 
       } else {
        
        var gid_valid = true;

        //Set the hex to the result of the entropy
        var myHexString = result.Entropy;

        //Create a buffer for the hex string
        var buffer = Buffer.from(myHexString);

        //Init the instance
        var instanceWithHex = addon.initWithHex(handle, buffer, buffer.length);

        //Start timer
        var start = now()

        addon.CalculateResultsAsync(instanceWithHex, radius, x, y, GID, filter, function(results) {
          //End timer
          var end = now()

          //Total time calculation
          var calc_time = (end).toFixed(2);

          //We create a array from the results of making the attractors. 
          cont.makeattractor(GID, results, gid_valid, true, calc_time, function(results){

            //Return the results so the message can be send to the main function.   
            callback(results);

          });
          
        });
      }
        
  }); // End qrng getentropy
  
}; // End function

process.on('message', async (limit) => {
  
  //We call the longComputation function, which will calculate the attractors.
  //On completion of calculating attractors we send the result to the main function. 
  longComputation(limit, function(result) {
  		process.send(result);
  });
  
});