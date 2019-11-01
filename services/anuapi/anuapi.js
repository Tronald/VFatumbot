//AnuAPI
const https = require('https')
var fs = require('fs');
const addon = require('../../build/Release/AttractFunctions');
var crypto = require('crypto');
var size_url = 520;
var length_url = 500;

var data = {}
var pool;

var exports = module.exports = {};

//Used for getting entropy from ANU
exports.getqrng  = function(callback){

var url = "https://qrng.anu.edu.au/API/jsonI.php?type=hex16&size="+size_url+"&length="+length_url;

  callanu(url, function(result) {
    callback(result)
  });

}

//Used for getting entropy from ANU by Size
exports.getsizeqrng  = function(size, callback){
if (size == 0){ callback(JSON.stringify(1)); return size}
else if (size < 286) {callback(JSON.stringify(2)); return size}
else if (size > 6000000) {callback(JSON.stringify(3)); return size}
else {
console.log("Start Entropy Reqeust of size: " + size)

//One size_url generates 2 hex characters for every number. 
//1 size_url generates 2 hex characters. 

//A big query is 260000 Characters (520*500)
var big_query;

//A small qeury' length is undetermined, block size is 520.
var small_query;

var results = [];

var size_url = 520;

//Do sum
var size_f = (size/2)

if (size_f-(520*500) < 0){ //Check if there is no big query possible, only a small query
  big_query = 0; //No big query
  var size_f = Math.trunc(size_f/520); //We remove the remainder.
  var size_f = size_f+1; //We add one

  small_query = size_f;

} else { //Check for big queries

  var big_query = Math.trunc(size_f/(520*500)) //This is the amount of big queries

  var size_f = size_f % (520*500); //We get the remainder
  
  if (size_f == 0) { //Check if remainder is zero

    small_query = size_f; //0 Small queries

  } else {

    var size_f = Math.trunc(size_f/520); //Remainder / 520 and we remove the fraction
    var size_f = size_f+1; //We add one

    small_query = size_f; //This is the amount of small queries
  }


}

//We send big_query x amount of times. 
if (big_query != 0 && small_query == 0){
  var length_url = 500;
  var url = "https://qrng.anu.edu.au/API/jsonI.php?type=hex16&size="+size_url+"&length="+length_url;
  

  callanubig(size, url, big_query, function(result) {
        callback(result)

  })

//We only have small queries.
} else if (small_query != 0 && big_query == 0) {
  var length = small_query;
  var url = "https://qrng.anu.edu.au/API/jsonI.php?type=hex16&size="+size_url+"&length="+length;


    callanu(size, length, url, function(result) {
        callback(result)
    });


} else if (big_query != 0 && small_query != 0){
  var length = small_query;
  var length_url = 500;
  var url = "https://qrng.anu.edu.au/API/jsonI.php?type=hex16&size="+size_url+"&length="+length_url;

  callanumulti(size, url, big_query, length, function(result) {
       callback(result)
  })


}

} //End if beginning checks
} //End Function

//Used for calling ANU Quantum Random Numbers Server
function callanu(size, length, url, callback){
  https.get(url, function(res){
    var body = '';

      res.on('data', function(chunk){
          body += chunk;
      });

      res.on('end', function(){
          
          var object_entropy = undefined; 
          var fbResponse = JSON.parse(body);
          var tmpstring = fbResponse.data

        for (i = 0; i < length; i++){
          if(!object_entropy){
              object_entropy = tmpstring[i]
          } else {
            object_entropy += tmpstring[i]
          }         
        }

          var gid = crypto.createHash('sha256').update(object_entropy).digest('hex');

          var timestamp = Date.now(); 
          var entropy = object_entropy.substring(0,size); 
          if(pool == undefined){
              pool = object_entropy.substring(size, object_entropy.length); 
          } else {
             pool += object_entropy.substring(size, object_entropy.length); 
          }
          if(pool.length > 1000000){
              savePool();
          }

          var entropy_size = entropy.length;

          var myObj = {
            EntropySize: entropy_size,
            Timestamp: timestamp,
            Gid: gid,
            Entropy: entropy
          }

        console.log("Small query of " + length + " length done");
        console.log("Total entropy: " + object_entropy.length);
        console.log("Added to pool: " + (object_entropy.length - size));
        console.log("End Entropy Reqeust of size: " + entropy_size)

      fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
                  if (err) throw err;
            callback(myObj)
      }
);

    });
}).on('error', function(e){
      console.log("Got an error: ", e);
});


}

//Used for calling ANU Quantum Random Numbers Server
function callanubig(size, url, big_query, callback){
var results_anu = undefined; //This is a large array containing the entropy. 

for (i = 0; i < big_query; i++) {

  anu(url, function(result) {
    if(results_anu == undefined){results_anu = result; 
    } else { results_anu += result; }

    if(i == big_query){
      

      var gid = crypto.createHash('sha256').update(results_anu).digest('hex');

      var timestamp = Date.now(); 
      var entropy = results_anu.substring(0,size); 
        if(pool == undefined){
          pool = results_anu.substring(size, results_anu.length); 
        } else {
          pool += results_anu.substring(size, results_anu.length); 
        }
        if(pool.length > 1000000){
            savePool();
        }
          
        var entropy_size = entropy.length;

        var myObj = {
            EntropySize: entropy_size,
            Timestamp: timestamp,
            Gid: gid,
            Entropy: entropy
        }

        console.log(big_query + " amounts of big queries done");
        console.log("Total entropy: " + results_anu.length);
        console.log("Added to pool: " + (results_anu.length - size));
        console.log("End Entropy Reqeust of size: " + entropy_size)
        fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
        if (err) throw err;
          callback(myObj)
        });


  }
  })

} // End loop

} //End Function

//Used for calling ANU Quantum Random Numbers Server
function callanumulti(size, url, big_query, small_query_length, callback){
var results_anu = undefined; //This is a large array containing the entropy. 
var timesRun = 0;

for (i = 0; i < big_query; i++) {
 anu(url, function(result) {
      ++timesRun
    if(results_anu == undefined){
      results_anu = result; 
    } else {
      results_anu += result; }

    if(timesRun == big_query){
        var url = "https://qrng.anu.edu.au/API/jsonI.php?type=hex16&size="+size_url+"&length="+small_query_length;
        anu(url, function(result){
            results_anu += result;

            var gid = crypto.createHash('sha256').update(results_anu).digest('hex');

            var timestamp = Date.now(); 
            var entropy = results_anu.substring(0,size); 
              if(pool == undefined){
                pool = results_anu.substring(size, results_anu.length); 
              } else {
                pool += results_anu.substring(size, results_anu.length); 
              }
              if(pool.length > 1000000){
                 savePool();
              }

              var entropy_size = entropy.length;

              var myObj = {
                  EntropySize: entropy_size,
                  Timestamp: timestamp,
                  Gid: gid,
                  Entropy: entropy
              }

              console.log(big_query + " amounts of big queries done");
              console.log("Small query of " + small_query_length + " length done");
              console.log("Total entropy: " + results_anu.length);
              console.log("Added to pool: " + (results_anu.length - size));
              console.log("End Entropy Reqeust of size: " + entropy_size)

              fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
              if (err) throw err;
                callback(myObj)
              });


              }) //End anu function

  }
  })

} // End loop

} //End Function

function anu(url, callback){

https.get(url, function(res){
    var body = '';

      res.on('data', function(chunk){
          body += chunk;
      });

      res.on('end', function(){

          var object_entropy = undefined; 
          var fbResponse = JSON.parse(body);
          var tmpstring = fbResponse.data

        for (v = 0; v < length_url; v++){
          if(!object_entropy){
              object_entropy = tmpstring[v]
          } else {
            object_entropy += tmpstring[v]
          }         
        }

        callback(object_entropy)

    });


}).on('error', function(e){
      console.log("Got an error: ", e);
});



}

//Used for setting own entropy
exports.saveentropy = function gettoken(entropy, size, gid, timestamp, callback){

        //Create object to store results
        var myObj = {
          EntropySize: size,
          Timestamp: timestamp,
          Gid: gid,
          Entropy: entropy
        }

        //Write file to disk by GID
        fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
          if (err) throw err;
            console.log('complete');
            callback(myObj);
        });

       
}

//Used for getting entropy with GID
exports.getentropy = function gettoken(gid, callback){

var obj = fs.readFile ('./services/entropy/'+gid+".hex", "utf8", function(err, data) {
          if (err){
              callback(JSON.stringify(1));
          } else {
              callback(JSON.parse(data));
          }
        });

} 

//Used for getting entropy with GID
exports.getpool = function(callback){
var timestamp = Date.now(); 
if(pool == undefined){
  gid = 0;
  var length_pool = 0;
} else {
  var gid = crypto.createHash('sha256').update(pool).digest('hex');

  var length_pool = pool.length;
}

var myObj = {
    GidCurrentPool: gid,
    TimestampReqeustPool: timestamp,
    PoolSize: length_pool,
    PoolEntropy: pool
  }
pool = undefined;
//Write file to disk by GID
fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
if (err) throw err;
  callback(myObj);
});

}

//Used for getting entropy with GID
function savePool(){
var timestamp = Date.now(); 
if(pool == undefined){
  gid = 0;
  var length_pool = 0;
} else {
  var gid = crypto.createHash('sha256').update(pool).digest('hex');

  var length_pool = pool.length;
}

var myObj = {
    GidCurrentPool: gid,
    TimestampReqeustPool: timestamp,
    PoolSize: length_pool,
    PoolEntropy: pool
  }
pool = undefined;
//Write file to disk by GID
fs.writeFile ('./services/entropy/'+gid+".hex", JSON.stringify(myObj, null, 2), function(err) {
if (err) throw err;
  console.log('Pool Saved');
});

}


//end FS