"""Native Unity YAML from the original type tree, including nested serialization versions.

Callers must resolve every non-null PPtr and preserve provenance before importing.
This module does not execute or copy original MonoBehaviour code.
"""
import json,math

def native_tree(node,value,resolve_pointer,path=''):
 if node.m_Type.startswith('PPtr<'):return resolve_pointer(value,path)
 if isinstance(value,dict):
  result={}
  if node.m_Version>1:result['serializedVersion']=node.m_Version
  children={c.m_Name:c for c in node.m_Children}
  for key,item in value.items():
   if key not in children:raise ValueError(f'Unmapped original field {path}.{key}')
   result[key]=native_tree(children[key],item,resolve_pointer,path+'.'+key)
  return result
 if isinstance(value,(list,tuple)):
  array=next((c for c in node.m_Children if c.m_Type=='Array'),node)
  data=next((c for c in array.m_Children if c.m_Name=='data'),None)
  if data is None:raise ValueError(f'Unmapped original array {path}')
  return [native_tree(data,v,resolve_pointer,f'{path}[{i}]') for i,v in enumerate(value)]
 return value

def scalar(value):
 if isinstance(value,bool):return '1' if value else '0'
 if value is None:return ''
 if isinstance(value,float) and not math.isfinite(value):return 'NaN' if math.isnan(value) else 'Infinity' if value>0 else '-Infinity'
 return json.dumps(value,ensure_ascii=False)

def yaml_lines(value,indent=0):
 prefix=' '*indent
 if isinstance(value,dict):
  for key,item in value.items():
   if isinstance(item,(dict,list)) and item:
    yield prefix+str(key)+':'
    yield from yaml_lines(item,indent+2)
   else:yield prefix+str(key)+': '+('{}' if isinstance(item,dict) else '[]' if isinstance(item,list) else scalar(item))
 elif isinstance(value,list):
  for item in value:
   if isinstance(item,(dict,list)) and item:
    lines=list(yaml_lines(item,indent+2));yield prefix+'- '+lines[0][indent+2:];yield from lines[1:]
   else:yield prefix+'- '+scalar(item)

def document(class_id,file_id,type_name,data):
 return f'--- !u!{class_id} &{file_id}\n{type_name}:\n'+'\n'.join(yaml_lines(data,2))+'\n'
