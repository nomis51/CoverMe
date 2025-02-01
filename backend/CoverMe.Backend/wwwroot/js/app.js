(function(n){typeof define=="function"&&define.amd?define(n):n()})(function(){"use strict";const n="intellij";class h{static namespace=n;getProjectSettings(){if(window[n])return{projectRootPath:window[n].PROJECT_ROOT_PATH,channelId:window[n].CHANNEL_ID}}}const l="coverageTable";class a{static namespace=l;resizableDivs=[];currentColumn;nextColumn;pageX;currentColumnWidth;nextColumnWidth;initialize({id:e}){if(!e)return;const t=document.getElementById(e);t&&this.makeResizable(t)}dispose(){this.resizableDivs.forEach(e=>e.addEventListener("mousedown",this.onMouseDown.bind(this))),this.resizableDivs.forEach(e=>e.addEventListener("mouseover",this.onMouseOver.bind(this))),this.resizableDivs.forEach(e=>e.addEventListener("mouseout",this.onMouseOut.bind(this))),document.addEventListener("mousemove",this.onMouseMove.bind(this)),document.addEventListener("mouseup",this.onMouseUp.bind(this))}makeResizable(e){const t=e.getElementsByTagName("tr");if(!t||t.length===0)return;const s=t[0];if(s.children.length===0)return;const r=s.children,m=e.offsetHeight;for(let o=0;o<r.length;++o){const u=this.createResizableDiv(m);this.resizableDivs.push(u),r[o].appendChild(u),r[o].style.position="relative",this.setListeners(u)}}createResizableDiv(e){const t=document.createElement("div");return t.style.top="0",t.style.right="0",t.style.width="5px",t.style.position="absolute",t.style.cursor="col-resize",t.style.userSelect="none",t.style.height=e+"px",t}setListeners(e){e.addEventListener("mousedown",this.onMouseDown.bind(this)),e.addEventListener("mouseover",this.onMouseOver.bind(this)),e.addEventListener("mouseout",this.onMouseOut.bind(this)),document.addEventListener("mousemove",this.onMouseMove.bind(this)),document.addEventListener("mouseup",this.onMouseUp.bind(this))}getPaddingDiff(e){if(this.getStyleValue(e,"box-sizing")==="border-box")return 0;const t=this.getStyleValue(e,"padding-left"),s=this.getStyleValue(e,"padding-right");return parseInt(t)+parseInt(s)}getStyleValue(e,t){return window.getComputedStyle(e).getPropertyValue(t)}onMouseUp(e){this.currentColumn=void 0,this.nextColumn=void 0,this.pageX=void 0,this.currentColumnWidth=void 0,this.nextColumnWidth=void 0}onMouseOut(e){e.target.style.borderRight=""}onMouseMove(e){if(!this.currentColumn)return;const t=e.pageX-this.pageX;this.nextColumn&&(this.nextColumn.style.width=this.nextColumnWidth-t+"px"),this.currentColumn.style.width=this.currentColumnWidth+t+"px"}onMouseOver(e){e.target.style.borderRight="2px solid gray"}onMouseDown(e){this.currentColumn=e.target.parentElement,this.nextColumn=this.currentColumn.nextElementSibling,this.pageX=e.pageX;const t=this.getPaddingDiff(this.currentColumn);this.currentColumnWidth=this.currentColumn.offsetWidth-t,this.nextColumn&&(this.nextColumnWidth=this.nextColumn.offsetWidth-t)}}const c=[h,a],d="coverme";window[d]={};for(const i of c)window[d][i.namespace]=new i});
