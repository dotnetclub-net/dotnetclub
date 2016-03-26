
var mergeStream = require('merge-stream'),
    onEndOfStream = require('end-of-stream'),
    consumeStream = require('stream-consume');

var gulp = require('gulp'),
    sass = require('gulp-sass'),
    eslint = require('gulp-eslint'),
    babel = require('gulp-babel'),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    rename = require('gulp-rename'),
    deleteRecursively = require("gulp-rimraf");

var paths = { webroot: "./" };
definePaths();


defineTask('lint', function() {
    // see eslint documation: http://eslint.org/docs/user-guide/configuring
    // see gulp-eslint project: https://github.com/adametry/gulp-eslint/
    var eslintOptions = {
        extends: 'eslint:recommended',
        ecmaVersion: 6,
        ecmaFeatures: {
            'modules': true,
            'impliedStrict': true
        },
        rules: {
        },
        globals: {
            'jQuery': true,
            '$': true
        },
        envs: [
            'browser',
            'es6'
        ]
    };

    // ESLint ignores files with "js" path.
    return gulp.src([paths.es6Source, paths.jsAll, '!' + paths.jsGenerated])
        .pipe(eslint(eslintOptions))
        .pipe(eslint.format())
        .pipe(eslint.failAfterError());
});


defineTask('sass', function() {
    return gulp.src(paths.scssSource)
        .pipe(sass())
        .pipe(gulp.dest(paths.cssDist));
});

defineTask('babel', function() {
    return gulp.src(paths.es6Source)
        .pipe(babel())
        .pipe(gulp.dest(paths.jsDist));
});

defineTask("use-libs", function *() {
    // bootstrap
    var bootstrapDest = paths.libDist + '/bootstrap-sass';
    var bootstrapDestStyle = bootstrapDest + '/stylesheets';

    // copy files
    yield gulp.src([paths.libSource + '/bootstrap-sass/assets/**/*' ]).pipe(gulp.dest(bootstrapDest));
    // build bootstrap
    yield gulp.src(bootstrapDestStyle + '/_bootstrap.scss')
        .pipe(rename('bootstrap.scss'))
        .pipe(sass())
        .pipe(gulp.dest(bootstrapDestStyle));
    yield gulp.src(bootstrapDestStyle + '/bootstrap.css', { base: "./" })
        .pipe(cssmin())
        .pipe(rename(function (path) {
            path.extname = ".min.css";
        }))
        .pipe(gulp.dest('.'));
    // clean useless files
    yield mergeStream([
        enumerateFiles(bootstrapDestStyle + '/bootstrap').pipe(deleteRecursively()),
        enumerateFiles(bootstrapDestStyle + '/*.scss').pipe(deleteRecursively()),
        enumerateFiles(bootstrapDest + '/javascripts/bootstrap').pipe(deleteRecursively())
    ]);


    // jquery
    yield gulp.src([paths.libSource + '/jquery/dist/**/*' ]).pipe(gulp.dest(paths.libDist + '/jquery'));
});

defineTask("minify", function *() {
    yield gulp.src([paths.cssGenerated, '!' + paths.cssMin],  { base: "./" })
        .pipe(cssmin())
        .pipe(rename(function (path) {
            path.extname = ".min.css";
        }))
        .pipe(gulp.dest('.'));

    yield gulp.src([paths.jsAll, '!' + paths.jsMin],  { base: "./" })
        .pipe(uglify())
        .pipe(rename(function (path) {
            path.extname = ".min.js";
        }))
        .pipe(gulp.dest("."));
});

defineTask('release', function (callback) {
    // todo: add release logic (package; remove dev files)
    callback();
});

defineTask('clean', function*() {
    // clean all generated files by compilers like babel, and sass
    var deljs = enumerateFiles(paths.jsGenerated).pipe(deleteRecursively());
    var delcss = enumerateFiles(paths.cssGenerated).pipe(deleteRecursively());
    var dellibs = enumerateFiles(paths.libDist).pipe(deleteRecursively());

    // use callback in synchronous tasks
    // see http://schickling.me/synchronous-tasks-gulp/
    yield mergeStream(deljs, delcss, dellibs);
});

defineTask('clean-all', ['clean'], function(callback) {
    // clean all node_modules
    // clean all lib/source
    enumerateFiles(paths.libSource).pipe(deleteRecursively());
    enumerateFiles(paths.nodeModules).pipe(deleteRecursively());

    callback();
});

function enumerateFiles(glob){
    return gulp.src(glob, { read: false });
}

function definePaths() {
    definePathPattern('libSource', 'lib/source');
    definePathPattern('libDist', 'lib/dist');
    definePathPattern('nodeModules', 'node_modules');

    definePathPattern('es6Source', 'scripts/**/*.es6');
    definePathPattern('jsDist', 'scripts/js');
    definePathPattern('jsGenerated', 'scripts/js/**/*.js');
    definePathPattern('jsAll', 'scripts/**/*.js');
    definePathPattern('jsMin', 'scripts/**/*.min.js');


    definePathPattern('scssSource', 'stylesheets/scss/**/*.scss');
    definePathPattern('cssDist', 'stylesheets/css');
    definePathPattern('cssGenerated', 'stylesheets/css/**/*.css');
    definePathPattern('cssMin', 'stylesheets/css/**/*.min.css');

    function definePathPattern(name, glob){
        if(glob[0] === '/'){
            glob = glob.substr(1);
        }

        paths[name] = paths.webroot + glob;
    }
}

function defineTask(name, dependencies, taskFn){
    if(arguments.length < 3 && typeof dependencies === 'function'){
        taskFn = dependencies;
        dependencies = [];
    }

    if(!taskFn){
        gulp.task(name, dependencies);
    }else if(taskFn.constructor.name === 'GeneratorFunction'){
        gulp.task(name, dependencies, function (callback) {
            arrangeStreams(taskFn(), callback);
        });
    }else {
        gulp.task(name, dependencies, taskFn);
    }

    function arrangeStreams(gen, cb){
        var context = gen.next();
        if(context.done){ cb(); return; }

        var stream = context.value;
        if (stream && typeof stream.pipe === 'function'){
            // consume and wait for completion of a stream: https://github.com/robrich/orchestrator/blob/master/lib/runTask.js
            onEndOfStream(stream, { error: true, readable: stream.readable, writable: stream.writable && !stream.readable }, function(err){
                if(err){ cb(err); return; }
                arrangeStreams(gen, cb);
            });
            consumeStream(stream);
        }
    }
}


/*
*
* JavaScript build steps:
*
* Checking & Compiling:
  * npm install
  * bower install
  * eslint
  * compile scss to css
  * compile es6 to js


* Tests
  * js tests
  * cucumber js tests

* Release
  * compress css files
  * compress js files
  *
  * #combining css files
  * #combining js files
  * strip dev files before packaging
    - Root path: only allow favicon.ico, web.config
    - remove .idea
    - remove node_modules
    - remove lib/source
    - remove stylesheets/scss
    - remove scripts/es6
*
*
* */

// Task chains
defineTask('compile', ['clean', 'use-libs', 'babel', 'sass']);
defineTask('release', ['lint', 'compile', 'minify', 'package']);
