
// this is a webpack entry point file

import '../lib/node_modules/jquery';
import '../lib/node_modules/bootstrap';
import '../lib/node_modules/summernote';
import '../lib/node_modules/summernote/dist/lang/summernote-zh-CN.min';

import '../lib/node_modules/turndown';
import '../lib/node_modules/prismjs';

import * as utils from './functions'
import * as mdeditor from './markdown-editor'
import * as topic from './topic'
import * as user from './user/index'



window.jQuery = window.$ = jQuery;
window.DiscussionApp = { utils, mdeditor, topic, user  };
