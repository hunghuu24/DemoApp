// Extract original filled stroke geometry from the locally decoded Nhom4.fig, without redrawing icons.
// Usage: node tools/Export-FigmaCatalogIcons.cjs <decoded.json>
const fs = require('fs');
const path = require('path');
const source = process.argv[2];
if (!source) throw new Error('Provide the decoded .fig JSON path');
const data = JSON.parse(fs.readFileSync(source, 'utf8'));
const id = g => g ? `${g.sessionID}:${g.localID}` : null;
const commerce = process.argv[3] === 'commerce';
const names = commerce ? ['paperclip', 'send'] : ['armchair', 'smartphone', 'shirt', 'cooking-pot', 'book-open', 'bike'];
const output = path.resolve(__dirname, '../Android/app/src/main/res/drawable');
const manifest = [];
function commands(index) {
  const buffer = Buffer.from(Object.values(data.blobs[index].bytes));
  let offset = 0;
  const result = [];
  while (offset < buffer.length) {
    const opcode = buffer[offset++];
    const command = { 0: ['Z', 0], 1: ['M', 2], 2: ['L', 2], 3: ['Q', 4], 4: ['C', 6] }[opcode];
    if (!command) throw new Error(`Unsupported path opcode ${opcode}`);
    const values = [];
    for (let i = 0; i < command[1]; i++) {
      const value = buffer.readFloatLE(offset); offset += 4;
      if (!Number.isFinite(value) || Math.abs(value) > 1000) throw new Error('Invalid vector coordinate');
      values.push(Number(value.toFixed(5)));
    }
    result.push(command[0] + values.join(','));
  }
  if (offset !== buffer.length) throw new Error('Truncated path data');
  return result.join(' ');
}
for (const name of names) {
  const node = commerce ? data.nodeChanges.find(n => id(n.guid) === (name === 'send' ? '205:118304' : '205:118300'))
    : data.nodeChanges.find(n => n.name === name && n.size?.x === 30 && n.size?.y === 30);
  if (!node) throw new Error(`Missing original icon ${name}`);
  const vectors = data.nodeChanges.filter(n => id(n.parentIndex?.guid) === id(node.guid) && n.type === 'VECTOR');
  if (!vectors.length) throw new Error(`No vector child for ${name}`);
  const groups = vectors.map(vector => {
    const transform = vector.transform;
    if (transform.m00 !== 1 || transform.m11 !== 1 || transform.m01 !== 0 || transform.m10 !== 0) throw new Error('Unexpected icon transform');
    return `    <group android:translateX="${transform.m02}" android:translateY="${transform.m12}">\n` +
      vector.strokeGeometry.map(geometry => `        <path android:fillColor="#28734F" android:fillType="${geometry.windingRule === 'ODD' ? 'evenOdd' : 'nonZero'}" android:pathData="${commands(geometry.commandsBlob)}"/>`).join('\n') + '\n    </group>';
  });
  const file = `ic_fg_${commerce ? 'commerce_' : ''}${name.replaceAll('-', '_')}.xml`;
  fs.writeFileSync(path.join(output, file), `<vector xmlns:android="http://schemas.android.com/apk/res/android" android:width="${node.size.x}dp" android:height="${node.size.y}dp" android:viewportWidth="${node.size.x}" android:viewportHeight="${node.size.y}">\n${groups.join('\n')}\n</vector>\n`);
  manifest.push({ name, nodeId: id(node.guid), vectors: vectors.map(v => id(v.guid)), file, width: node.size.x, height: node.size.y });
}
fs.writeFileSync(path.resolve(__dirname, commerce ? '../docs/assets/commerce/icons.json' : '../docs/assets/catalog-icons.json'), JSON.stringify(manifest, null, 2) + '\n');
console.log(`Exported ${manifest.length} original Figma vector icons.`);
