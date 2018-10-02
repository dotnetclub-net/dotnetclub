const path = require('path');
const webpack = require("webpack");
const UglifyJsPlugin = require("uglifyjs-webpack-plugin");
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");

module.exports = (env, argv) =>  ({
    entry: {
        'js': './scripts/_entry-js.js',
        'css': './stylesheets/_entry-css.js'
    },
    resolve: {
        extensions: [".js", ".css", ".scss"],
        alias: {
            jquery: path.resolve(__dirname, 'lib/node_modules/jquery'),
        }
    },
    output:{
        path: path.resolve(__dirname, './dist'),
        filename:'packed-[name].' + (argv.mode === 'development'  ? 'js' : 'min.js')
    },
    module:{
        rules:[
            {
                test: /\.scss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    'css-loader',
                    'sass-loader'
                ]
            },
            {
                test: /\.(jpe?g|png|gif|svg|woff2?|ttf|eot|ico|mp3|wav|amr)$/,
                use: [
                    'file-loader'
                ]
            },
            {
                test: /\.css$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    'css-loader'
                ]
            },
        ]
    },
    optimization: {
        minimizer: [
            new UglifyJsPlugin({
                cache: true,
                parallel: true,
                sourceMap: true
            }),
            new OptimizeCSSAssetsPlugin({})
        ],
        splitChunks: {
            minSize: 1,
            maxSize: 0,
            minChunks: 1,
            cacheGroups: {
                jsvendor: {
                    test: /[\\/]node_modules[\\/].+\.js$/,
                    name: 'js-vendor',
                    chunks: 'all'
                },
                cssvendor: {
                    test: /[\\/]node_modules[\\/].+\.css$/,
                    name: 'css-vendor',
                    chunks: 'all'
                },
            }
        }
    },
    plugins: [
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery'
        }),
        new MiniCssExtractPlugin({
            filename: 'packed-' + (argv.mode === 'development'  ? '[name].css' : '[name].min.css'),
            chunkFilename: 'packed-' + (argv.mode === 'development' ? '[name].css' : '[name].min.css')
        }),
        new webpack.IgnorePlugin(/^codemirror$/)
    ]
});