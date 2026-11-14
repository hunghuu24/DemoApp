const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'..'),source=path.join(root,'docs/assets/smart-swap');
const attrs=s=>Object.fromEntries([...s.matchAll(/([\w:-]+)="([^"]*)"/g)].map(x=>[x[1],x[2]]));
const color=value=>({white:'#FFFFFF',black:'#000000',none:'#00000000'}[value]||value);
const manifest=[];
for(const name of ['back','down','check']){
 const svg=fs.readFileSync(path.join(source,'find-'+name+'.svg'),'utf8'),a=attrs(svg.match(/<svg\b([^>]+)>/)[1]);
 const [x,y,w,h]=a.viewBox.split(' ').map(Number);if(x||y)throw Error('Unexpected viewBox origin');
 const paths=[...svg.matchAll(/<path\b([^>]+)>/g)].map(p=>{const b=attrs(p[1]);return `<path android:pathData="${b.d}" android:fillColor="${color(b.fill||'#00000000')}"`+
 (b.stroke?` android:strokeColor="${color(b.stroke)}" android:strokeWidth="${b['stroke-width']||1}" android:strokeLineCap="${b['stroke-linecap']||'butt'}" android:strokeLineJoin="${b['stroke-linejoin']||'miter'}"`:'')+' />';});
 if(!paths.length)throw Error('Missing vector paths');const filename=`ic_fg_smart_${name}.xml`;
 fs.writeFileSync(path.join(root,'Android/app/src/main/res/drawable',filename),`<vector xmlns:android="http://schemas.android.com/apk/res/android" android:width="${a.width}dp" android:height="${a.height}dp" android:viewportWidth="${w}" android:viewportHeight="${h}">\n${paths.join('\n')}\n</vector>\n`);
 manifest.push({source:'find-'+name+'.svg',drawable:filename,width:a.width,height:a.height,paths:paths.length});
}
fs.writeFileSync(path.join(source,'assets.json'),JSON.stringify({frame:'205:115234',assets:manifest,nativeStatusBar:true,dynamicProductImages:true,diagram:'205:115119'},null,2)+'\n');
console.log(JSON.stringify(manifest));
