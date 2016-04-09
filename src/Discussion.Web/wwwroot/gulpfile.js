
var gulp = require('gulp');
var mergeStream = require('merge-stream');
var runSequence = require('run-sequence');
require('gulp-awaitable-tasks')(gulp);



var sass = require('gulp-sass'),
    eslint = require('gulp-eslint'),
    babel = require('gulp-babel'),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    rename = require('gulp-rename'),
    deleteRecursively = require("gulp-rimraf");

var paths = { webroot: "./" };
definePaths();


gulp.task('lint', function() {
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


gulp.task('sass', function() {
    return gulp.src(paths.scssSource)
        .pipe(sass({sourceComments: true}))
        .pipe(gulp.dest(paths.cssDist));
});

gulp.task('babel', function() {
    var compileES6 = gulp.src(paths.es6Source)
        .pipe(babel())
        .pipe(gulp.dest(paths.jsDist));

    var copyJS = gulp.src(paths.jsSource)
        .pipe(gulp.dest(paths.jsDist));

    return mergeStream(compileES6, copyJS);
});

gulp.task("use-libs", function *() {
    // bootstrap
    var bootstrapDest = paths.libDist + '/bootstrap-sass';
    var bootstrapDestStyle = bootstrapDest + '/stylesheets';

    // copy files
    yield gulp.src([paths.libSource + '/bootstrap-sass/assets/**/*' ]).pipe(gulp.dest(bootstrapDest));
    // build bootstrap
    yield gulp.src(bootstrapDestStyle + '/_bootstrap.scss')
        .pipe(rename('bootstrap.scss'))
        .pipe(sass({sourceComments: true}))
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

gulp.task("minify", function *() {
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

gulp.task('release', function (callback) {
    // todo: add release logic (package; remove dev files)
    callback();
});

gulp.task('clean', function*() {
    // clean all generated files by compilers like babel, and sass
    var deljs = enumerateFiles(paths.jsGenerated).pipe(deleteRecursively());
    var delcss = enumerateFiles(paths.cssGenerated).pipe(deleteRecursively());
    var dellibs = enumerateFiles(paths.libDist).pipe(deleteRecursively());

    // use callback in synchronous tasks
    // see http://schickling.me/synchronous-tasks-gulp/
    return mergeStream(deljs, delcss, dellibs);
});

gulp.task('clean-all', ['clean'], function(callback) {
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

    definePathPattern('es6Source', 'scripts/es6/**/*.es6');
    definePathPattern('jsSource', 'scripts/es6/**/*.js');
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
gulp.task('compile', function (callback) {
    runSequence('clean', ['babel', 'sass'], 'use-libs', callback);
});
gulp.task('release', ['lint', 'compile'], function (callback) {
    runSequence('minify', callback);
    // todo: package
});

