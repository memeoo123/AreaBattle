import contextlib,io,runpy,json,sys
sys.stdout.reconfigure(encoding='utf8')
with contextlib.redirect_stdout(io.StringIO()):
    d=runpy.run_path('analysis/controls_disassemble.py')
ms,ts,md=d['ms'],d['ts'],d['md']
root=next((i,t) for i,t in enumerate(ts) if ms(t[0])=='WayLineControl')
print('Root',root)
for i,t in enumerate(ts):
    if t[3] in (root[0],root[1][2]):
        print(i,ms(t[0]),'decl',t[3], 'methods',[(j,ms(md[j][0])) for j in range(t[9],t[9]+t[16])])
