from pathlib import Path
import re,sys
p=Path(sys.argv[1]);ls=p.read_text(encoding='utf8').splitlines();st=[];end={};target={}
for i,s in enumerate(ls):
 if re.search(r'\b(block|if|loop) \[',s):st.append(i)
 elif 'end []' in s and st:end[st.pop()]=i
for i,s in enumerate(ls):
 if re.search(r'\b(block|if|loop) \[',s):st.append(i)
 elif 'end []' in s and st:st.pop()
 elif m:=re.search(r'\bbr(?:_if)? \[(\d+)\]',s):
  j=st[-1-int(m[1])];target[i]=ls[end.get(j,j)][:8]
for i,s in enumerate(ls):
 if re.match(r'^[0-9a-f]{8} ',s) and (len(sys.argv)<4 or int(sys.argv[2],16)<=int(s[:8],16)<=int(sys.argv[3],16)):
  print(re.sub(r'  +',' ',s)+(' TARGET '+target[i] if i in target else ''))
