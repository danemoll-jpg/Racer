import sys; sys.argv=[sys.argv[0], sys.argv[1]]
exec(open(__file__.replace('viz.py','place.py'),encoding='utf-8').read().split("if __name__ == '__main__':")[0])
from PIL import Image
img=np.zeros((NZ,NX,3),np.uint8)
col={'G':(90,130,70),'R':(200,200,200),'T':(20,70,20),'B':(200,80,80),'E':(60,40,30),'N':(0,0,0)}
for k,c in col.items(): img[cls==k]=c
img[obs & (cls!='T')]=(230,150,60)
img[d_line<35]=img[d_line<35]//2+np.array([0,0,120],np.uint8)
img[d_edge<150]=img[d_edge<150]//2+np.array([60,0,0],np.uint8)
Image.fromarray(img[::-1]).resize((NX*2,NZ*2),Image.NEAREST).save(survey/'classes.png')
print('T cells',(cls=='T').sum(),'O',obs.sum(),'R',(cls=='R').sum(),'G',(cls=='G').sum())
