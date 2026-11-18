import assert from 'node:assert/strict';
import {test} from 'node:test';
import {fileURLToPath} from 'node:url';
import React from 'react';
import {renderToStaticMarkup} from 'react-dom/server';
import {loadUI} from './helpers/load-ui.mjs';
const {MessageText}=loadUI(fileURLToPath(new URL('../src/widgets/ticket-details/ui/message-text.tsx',import.meta.url)));
const render=text=>renderToStaticMarkup(React.createElement(MessageText,{text}));
test('message formatting supports Markdown and sanitized legacy HTML',()=>{
 const html=render('**Bold** <u>Underline</u> <mark>Highlighted</mark>');
 assert.match(html,/<strong>Bold<\/strong>/);assert.match(html,/<u>Underline<\/u>/);assert.match(html,/<mark>Highlighted<\/mark>/);
});
test('messages strip executable HTML, event handlers and unsafe links',()=>{
 const html=render('<script>alert(1)</script><img src="x" onerror="alert(2)"><a href="javascript:alert(3)" onclick="alert(4)">unsafe</a><iframe src="https://example.test"></iframe>');
 assert.doesNotMatch(html,/<script|onerror|onclick|javascript:|<iframe|<img/);assert.match(html,/unsafe/);
});
