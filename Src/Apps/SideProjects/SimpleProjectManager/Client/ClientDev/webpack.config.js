const path = require('path');
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    entry: {
        index: './src/js/index.ts'
    },
    output: {
        filename: 'bundle.js',
        path: path.resolve(__dirname, '..', 'wwwroot', 'dist'),
        clean: true
    },
    devtool: 'inline-source-map',
    mode: 'development', //development
    module: {
        rules: [
            { 
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/,
            },
            { 
                test: /\.css$/, 
                use: ['style-loader', 'css-loader'] },
            { 
                test: /\.eot(\?v=\d+\.\d+\.\d+)?$/, 
                use: ['file-loader'] },
            {
                test: /\.(woff|woff2)$/, use: [
                    {
                        loader: 'url-loader',
                        options: {
                            limit: 5000,
                        },
                    },
                ]
            },
            {
                test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/, use: [
                    {
                        loader: 'url-loader',
                        options: {
                            limit: 10000,
                            mimetype: 'application/octet-stream',
                        },
                    },
                ]
            },
            {
                test: /\.svg(\?v=\d+\.\d+\.\d+)?$/, use: [
                    {
                        loader: 'url-loader',
                        options: {
                            limit: 10000,
                            mimetype: 'image/svg+xml',
                        },
                    },
                ]
            },
        ]
    },
    resolve: {
        extensions: ['.tsx', '.ts', '.js'],
    },
};