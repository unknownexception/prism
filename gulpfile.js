'use strict';

var gulp = require('gulp');

// Load plugins
var $ = require('gulp-load-plugins')();

// Scripts
gulp.task('scripts', function() {
  return gulp.src('app/scripts/app.js')
    .pipe($.browserify({
      insertGlobals: true,
      transform: ['reactify']
    }))
    .pipe(gulp.dest('dist/scripts'))
    .pipe($.size())
    .pipe($.connect.reload());
});

gulp.task('jade', function() {
  return gulp.src('app/template/*.jade')
    .pipe($.jade({
      pretty: true
    }))
    .pipe(gulp.dest('dist'))
    .pipe($.connect.reload());
});

// HTML
gulp.task('html', function() {
  return gulp.src('app/*.html')
    .pipe($.useref())
    .pipe(gulp.dest('dist'))
    .pipe($.size())
    .pipe($.connect.reload());
});

// Images
gulp.task('images', function() {
  return gulp.src('app/images/**/*')
    .pipe($.cache($.imagemin({
      optimizationLevel: 3,
      progressive: true,
      interlaced: true
    })))
    .pipe(gulp.dest('dist/images'))
    .pipe($.size())
    .pipe($.connect.reload());
});

gulp.task('test', function() {
  require('common-node').run('./node_modules/.bin/_mocha'); //common-node ./node_modules/.bin/_mocha
});

// Clean
gulp.task('clean', function() {
  return gulp.src(['dist/styles', 'dist/scripts', 'dist/images'], {
    read: false
  }).pipe($.clean());
});

// Bundle
gulp.task('bundle', ['scripts', 'styles', 'bower'], function() {
  return gulp.src('./app/*.html')
    .pipe($.useref.assets())
    .pipe($.useref.restore())
    .pipe($.useref())
    .pipe(gulp.dest('dist'));
});

// Build
gulp.task('build', ['html', 'bundle', 'images']);

// Default task
gulp.task('default', ['clean'], function() {
  gulp.start('build');
});

gulp.task('flow', function() {
  return gulp.src([
    'app/server/**/**.js',
    ])
    .pipe($.flowtype({
      declarations: './app/interfaces'
    }))
    .pipe($.react({
      stripTypes: true,
      harmony: true
    }))
    // Output each file into the ./build/javascript/ directory
    .pipe(gulp.dest('./app/server.compiled/'));
});



//Connect
gulp.task('connect', ['flow'], $.connect.server({
  root: ['dist'],
  port: 9000,
  livereload: true,
  middleware: require('./app/server.compiled/index.js')
}));

// Bower helper
gulp.task('bower', function() {
  gulp.src('app/bower_components/**/*.js', {
      base: 'app/bower_components'
    })
    .pipe(gulp.dest('dist/bower_components/'));

});

gulp.task('styles', function() {
  gulp.src('app/styles/**/*.css')
    .pipe(gulp.dest('dist/styles/'))
    .pipe($.connect.reload());;

});


// Watch
gulp.task('watch', ['html', 'bundle', 'connect'], function() {

  // Watch server-side js files
  gulp.watch('app/server/**/*.js', ['connect']);

  // Watch .html files
  gulp.watch('app/*.html', ['html']);

  // Watch .jade files
  //gulp.watch('app/template/**/*.jade', ['jade', 'html']);

  // Watch .jade files
  gulp.watch('app/styles/main.css', ['styles']);

  // Watch .js files
  gulp.watch('app/scripts/**/*.js', ['scripts']);

  // Watch image files
  gulp.watch('app/images/**/*', ['images']);
});