"""Record source-backed skill artwork, selectable and duration-mask behavior."""
import json
from pathlib import Path
R=Path(__file__).parent/'targets/wxcf1394487200e48f/43'
e=json.loads((R/'generated/hud-evidence.json').read_text(encoding='utf8'))
ids={10349,15930,10348,5671,15935,15938,15937}
out={'target':{'appId':'wxcf1394487200e48f','version':'43'},'status':'static-source-confirmed-runtime-validation-pending',
 'functions':[f for f in e['functions'] if f['function'] in ids],
 'rules':[
 {'id':'persistent-skill-art','source':'SkillItem.InitializeComponent f15930@0x74d09f binds img_cover to field64; Init f10349@0x4bc058..0x4bc069 invokes UIExtension.SetSprite f1300 on this Image','behavior':'Assign recovered heroSkill image to img_cover; retain visibility before/during/after cast. btn_normal Image alpha is only 1/255 and is a click target, not artwork.'},
 {'id':'selectable-sibling','source':'SkillItem.btn_normal Button642661712402947268 m_TargetGraphic6068776914646622331','behavior':'Resolve actual PPtr to SkillItem/bg, not Button own transparent Image. All recovered buttons retain their serialized targetGraphic.'},
 {'id':'duration-mask','source':'SkillBase.Updata f10353@0x4bc6a6..0x4bc6b1 and 0x4bc706..0x4bc712; SkillItem.SetProgress f10348','behavior':'Player skill advances mask fillAmount=elapsed/duration, sets zero on completion; source Image Filled Radial360 Clockwise origin2, starts zero. No invented countdown text.'},
 {'id':'drag-artwork','source':'SkillControl down f15938@0x74de50..0x74deaf and move f15935@0x74d831..0x74d895; release f15937@0x74da03..0x74da0b; SkillItem.SetCoverPosition f5671','behavior':'Project pointer(x,y+50,z5) through UI camera and write img_cover worldPosition; on release restore localPosition zero. Helper1241=set_localPosition;1348=set_position.'}],
 'validation':'SkillInputValidation traverses all six commanders, checks visible artwork, serialized sibling target, drag projection/reset, duration mask against production clock.'}
(R/'generated/hud-skill-feedback.json').write_text(json.dumps(out,ensure_ascii=False,indent=2)+'\n',encoding='utf8')
print('Recorded',len(out['rules']),'source skill HUD feedback rules')
