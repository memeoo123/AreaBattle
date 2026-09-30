"""Generate the ordered ConfigMgr table/value members and source30413 load sequence."""
from pathlib import Path
import json,struct
p=Path('analysis/recover_outgame_manager_registry.py');n={'__file__':str(p.resolve())};exec(p.read_text(encoding='utf8').split('rows=[]')[0],n)
r=json.loads((n['p']/'generated/outgame/CONFIG_INITIALIZATION_ROSTER.json').read_text(encoding='utf8'));names={};t=n['ts'][3907]
for j in range(t[18]):
 f=struct.unpack_from('<3i',n['b'],n['pairs'][11][0]+12*(t[8]+j));attrs=n['u'](n['u'](200288+4*f[1])+4)&65535
 if attrs&16:continue
 names[n['u'](n['u'](3823136+4*3907)+4*j)]=n['ms'](f[0])
lines=['// Generated from CONFIG_INITIALIZATION_ROSTER.json and source ConfigMgr30413.', 'using System.Collections.Generic;','using AreaBattle.OriginalConfig;','namespace AreaBattle','{','    public sealed partial class OutgameLegacyConfigManager','    {']
for row in r['rows']:
 name=names[row['ownerFieldOffset']];typ='OriginalConfig.'+row['sourceName'];assert name.isidentifier()
 lines+=['        public '+('readonly Dictionary<object,'+typ+'> '+name+'=new Dictionary<object,'+typ+'>();' if row['kind']=='table' else typ+' '+name+';')]
lines+=['        void ReadOriginalConfigs()','        {']
for row in r['rows']:
 name=names[row['ownerFieldOffset']];lines+=['            '+('reader.ReadTable('+name+');' if row['kind']=='table' else name+'=reader.ReadValue<OriginalConfig.'+row['sourceName']+'>();')]
lines+=['        }','    }','}'];Path('UnityProject/Assets/AreaBattle/Scripts/OutgameLegacyConfigManager.Tables.cs').write_text('\n'.join(lines)+'\n',encoding='utf8')
