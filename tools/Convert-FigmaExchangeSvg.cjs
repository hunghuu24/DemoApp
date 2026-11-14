// Convert the downloaded MCP SVGs to VectorDrawable, preserving root geometry and strokes.
const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'..'),source=path.join(root,'docs/assets/exchange');
const destination=path.join(root,'Android/app/src/main/res/drawable');
const names=['form-back','form-camera','form-down','form-city-down','form-switch'];
function attrs(text){return Object.fromEntries([...text.matchAll(/([\w:-]+)="([^"]*)"/g)].map(x=>[x[1],x[2]]));}
const metadata=[];
for(const name of names){
 const svg=fs.readFileSync(path.join(source,name+'.svg'),'utf8'),a=attrs(svg.match(/<svg\b([^>]+)>/)[1]);
 const [x,y,w,h]=a.viewBox.split(' ').map(Number);if(x||y)throw Error('Unexpected viewBox');
 let paths=[];
 for(const p of svg.matchAll(/<path\b([^>]+)>/g)){
  const b=attrs(p[1]);let value=`<path android:pathData="${b.d}" android:fillColor="${b.fill||'#00000000'}"`;
  if(b.stroke)value+=` android:strokeColor="${b.stroke}" android:strokeWidth="${b['stroke-width']||1}" android:strokeLineCap="${b['stroke-linecap']||'butt'}" android:strokeLineJoin="${b['stroke-linejoin']||'miter'}"`;
  paths.push(value+' />');
 }
 if(name==='form-switch'){
  paths=[`<path android:fillColor="#28734F" android:pathData="M12,0 H28 A12,12 0,0 1,40 12 A12,12 0,0 1,28 24 H12 A12,12 0,0 1,0 12 A12,12 0,0 1,12 0 Z" />`,
   `<path android:fillColor="#FFFFFF" android:pathData="M19,12 A9,9 0,1 0,37 12 A9,9 0,1 0,19 12 Z" />`];
 }
 if(!paths.length)throw Error('No paths for '+name);
 const filename='ic_fg_exchange_'+name.replace('form-','').replaceAll('-','_')+'.xml';
 fs.writeFileSync(path.join(destination,filename),`<?xml version="1.0" encoding="utf-8"?>\n<vector xmlns:android="http://schemas.android.com/apk/res/android" android:width="${a.width}dp" android:height="${a.height}dp" android:viewportWidth="${w}" android:viewportHeight="${h}">\n${paths.map(p=>'    '+p).join('\n')}\n</vector>\n`);
 metadata.push({source:name+'.svg',drawable:filename,width:a.width,height:a.height,paths:paths.length});
}
fs.writeFileSync(path.join(source,'assets.json'),JSON.stringify({figmaNode:'226:213',nativeStatusBar:true,assets:metadata},null,2)+'\n');
console.log('Converted '+metadata.length+' exchange assets.');
