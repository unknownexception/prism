/* @flow */
var CONF = require('config');
var FoursquareService = require('./service');
var FoursquareCalculator = require('./calculator');
var config = {
  'secrets': {
    'clientId': CONF.foursquare.clientId,
    'clientSecret': CONF.foursquare.clientSecret,
    'redirectUrl': CONF.app.host + '/api/v1/foursquare/callback'
  }
};
var foursquareLib = require('node-foursquare')(config);
var service = new FoursquareService(foursquareLib, {
  debug: process.NODE_ENV !== 'production'
}),
calculator = new FoursquareCalculator();

function Cache(){"use strict";}
              
                
                      
  Cache.prototype.init=function(service     , calculator)           {"use strict";
    live = {};
    player = {};
    //app.cache.live.i = 0;
    app.cache.checkinsData = service.getCheckins();
    console.log(fc);
    calculator.initFunctions.forEach(function(initFunc)  {
      initFunc(app.cache.live);
    });
  };


function Request(){"use strict";}
                     


var cache = new Cache();

module.exports.iterate = function(req         , content     , callback            )         {
  if (!cache) {
    if (req.queryParams.debug) {
      cache.init(service, calculator);
    } else {
      var error = new Error('Session expired');
      error.statusCode = 401;
      return callback(error);
    }
  }
  var currentCheckin = cache.checkinsData.checkins.items[cache.live.i];

  calculator.calculationFunctions.forEach(function(calcFunc) {
    calcFunc(currentCheckin, cache.live, cache.player); //currentCheckin, stats, socialPlayer
  });

  if (cache.checkinsData.checkins.items.length > cache.live.i) {
    cache.live.i++;
  } else {
    return callback(null, {
      final: 'final'
    });
  }

  return callback({
    live: cache.live,
    currentCheckin: currentCheckin,
    player: cache.player
  });

}
