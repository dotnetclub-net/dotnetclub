
var gulp = require('gulp'),
    sass = require('gulp-sass'),
    eslint = require('gulp-eslint'),
    babel = require('gulp-babel'),
    deleteRecursively = require("gulp-rimraf");

var paths = {
    webroot: "./"
};
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
        .pipe(sass())
        .pipe(gulp.dest(paths.cssDist));
});

gulp.task('babel', function() {
    return gulp.src(paths.es6Source)
        .pipe(babel())
        .pipe(gulp.dest(paths.jsDist));
});

gulp.task('clean', function(callback) {
    var force = {force: true};

    // clean all generated files by compilers like babel, and sass
    enumerateFiles(paths.jsGenerated).pipe(deleteRecursively(force));
    enumerateFiles(paths.cssGenerated).pipe(deleteRecursively(force));
    enumerateFiles(paths.libDist).pipe(deleteRecursively(force));

    // use callback in synchronous tasks
    // see http://schickling.me/synchronous-tasks-gulp/
    callback();
});

gulp.task('clean:all', ['clean'], function(callback) {
    var force = {force: true};

    // clean all node_modules
    // clean all lib/source
    enumerateFiles(paths.libSource).pipe(deleteRecursively(force));
    enumerateFiles(paths.nodeModules).pipe(deleteRecursively(force));

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

    definePathPattern('scssSource', 'stylesheets/scss/**/*.scss');
    definePathPattern('cssDist', 'stylesheets/css');
    definePathPattern('cssGenerated', 'stylesheets/css/**/*.css');

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

// Default Task
gulp.task('default', ['lint']);