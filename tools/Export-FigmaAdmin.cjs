// Original frame metadata from decoded Nhom4.fig. Does not change the design file.
const fs=require('fs'),path=require('path'),data=JSON.parse(fs.readFileSync(process.argv[2],'utf8'));
const id=g=>g?`${g.sessionID}:${g.localID}`:null,children=new Map();
for(const n of data.nodeChanges){const p=id(n.parentIndex?.guid);if(!children.has(p))children.set(p,[]);children.get(p).push(n);}
const read=n=>({id:id(n.guid),name:n.name,type:n.type,size:n.size,transform:n.transform,fill:n.fillPaints,radius:n.cornerRadius,font:n.fontName,fontSize:n.fontSize,lineHeight:n.lineHeight,text:n.textData?.characters,layout:n.stackMode,gap:n.stackSpacing,children:(children.get(id(n.guid))||[]).sort((a,b)=>a.parentIndex.position.localeCompare(b.parentIndex.position)).map(read)});
const frames=data.nodeChanges.filter(n=>n.name?.startsWith('Admin ·')&&n.size?.x===360),out=path.resolve(__dirname,'../docs/assets/admin');
fs.mkdirSync(out,{recursive:true});for(const n of frames)fs.writeFileSync(path.join(out,id(n.guid).replace(':','-')+'.json'),JSON.stringify(read(n),null,2)+'\n');
console.log(`Read ${frames.length} original admin frames.`);
