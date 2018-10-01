const path = require('path');
const webpack = require("webpack");
const CopyWebpackPlugin = require('copy-webpack-plugin');
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
        // 将这些文件复制到 dist/lib 中，以便 Development 模式中使用
        new CopyWebpackPlugin([
            { from: 'lib/node_modules/bootstrap/dist', to: 'dist/lib/bootstrap' },
            { from: 'lib/node_modules/jquery/dist', to: 'dist/lib/jquery' },
            { from: 'lib/node_modules/turndown/dist', to: 'dist/lib/turndown' },   // 也在 vender.js 中
            { from: 'lib/node_modules/prismjs/prism.js', to: 'dist/lib/prismjs/' },  // 也在 vender.js 中
            { from: 'lib/node_modules/prismjs/themes/prism.css', to: 'dist/lib/prismjs/themes/prism.css' },
            { from: 'lib/node_modules/summernote/dist', to: 'dist/lib/summernote' }
        ]),
        new webpack.ProvidePlugin({
            $: 'jquery',
            jQuery: 'jquery'
        }),
        new MiniCssExtractPlugin({
            filename: 'packed-' + (argv.mode === 'development'  ? '[name].css' : '[name].min.css'),
            chunkFilename: 'packed-' + (argv.mode === 'development' ? '[id].css' : '[id].min.css')
        }),
        new webpack.IgnorePlugin(/^codemirror$/)
    ]
});