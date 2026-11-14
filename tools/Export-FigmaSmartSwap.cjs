// Read-only extraction of original Smart Swap frames from decoded Nhom4.fig.
const fs=require('fs'),path=require('path');
const input=process.argv[2];if(!input)throw Error('Supply decoded.json');
const data=JSON.parse(fs.readFileSync(input,'utf8')),id=g=>g?`${g.sessionID}:${g.localID}`:null;
const nodes=new Map(data.nodeChanges.map(n=>[id(n.guid),n])),children=new Map();
for(const n of data.nodeChanges){const p=id(n.parentIndex?.guid);if(!children.has(p))children.set(p,[]);children.get(p).push(n);}
const frames=['205:115234','205:115155','205:115087','236:18','205:115021','205:114968','236:150','205:114915','205:114854'];
function read(n){return {id:id(n.guid),name:n.name,type:n.type,size:n.size,transform:n.transform,fill:n.fillPaints,stroke:n.strokePaints,radius:n.cornerRadius,
 font:n.fontName,fontSize:n.fontSize,lineHeight:n.lineHeight,text:n.textData?.characters,layout:n.stackMode,gap:n.stackSpacing,
 children:(children.get(id(n.guid))||[]).sort((a,b)=>a.parentIndex.position.localeCompare(b.parentIndex.position)).map(read)};}
const output=path.resolve(__dirname,'../docs/assets/smart-swap');fs.mkdirSync(output,{recursive:true});
for(const f of frames){const frame=nodes.get(f);if(!frame)throw Error('Missing '+f);fs.writeFileSync(path.join(output,f.replace(':','-')+'.json'),JSON.stringify(read(frame),null,2)+'\n');}
console.log(`Read ${frames.length} original 360x800 frames; geometry/text saved for review.`);
function commands(index){const b=Buffer.from(Object.values(data.blobs[index].bytes));let offset=0,result=[];
 while(offset<b.length){const op=b[offset++],c={0:['Z',0],1:['M',2],2:['L',2],3:['Q',4],4:['C',6]}[op];if(!c)throw Error('Unknown vector opcode');let values=[];
 for(let i=0;i<c[1];i++){values.push(Number(b.readFloatLE(offset).toFixed(5)));offset+=4;}result.push(c[0]+values.join(','));}return result.join(' ');}
const diagram=nodes.get('205:115119');
const paths=diagram.strokeGeometry.map(g=>`    <path android:fillColor="#28734F" android:pathData="${commands(g.commandsBlob)}"/>`).join('\n');
if(!paths)throw Error('Missing original diagram geometry');
fs.writeFileSync(path.resolve(output,'../../../Android/app/src/main/res/drawable/ic_fg_smart_cycle.xml'),
 `<vector xmlns:android="http://schemas.android.com/apk/res/android" android:width="288dp" android:height="52dp" android:viewportWidth="288" android:viewportHeight="52">\n${paths}\n</vector>\n`);
