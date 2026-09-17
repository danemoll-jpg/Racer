const fs=require('fs');const sharp=require('C:/Users/danmo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/sharp');
(async()=>{for(const label of ['before','after'])await sharp(`Docs/CR016-017/${label}-annotated.svg`).png().toFile(`Docs/CR016-017/${label}-annotated.png`);})();
