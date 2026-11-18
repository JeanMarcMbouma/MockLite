# Render the technical walkthrough: python render-walkthrough.py (requires PyMuPDF).
from pathlib import Path
import html,re,fitz
from html.parser import HTMLParser
root=Path(__file__).resolve().parent
class Runs(HTMLParser):
 def __init__(self): super().__init__();self.color='#edf3ff';self.out=[]
 def handle_starttag(self,tag,attrs):
  if tag=='span': self.color={'kw':'#b8a8ff','type':'#7ce6c5','str':'#f9cf92','comment':'#93a5bf','method':'#96caff'}.get(dict(attrs).get('class'),'#edf3ff')
 def handle_endtag(self,tag):
  if tag=='span':self.color='#edf3ff'
 def handle_data(self,data):self.out.append((self.color,data))
s=['<svg xmlns="http://www.w3.org/2000/svg" width="1440" height="940" viewBox="0 0 1440 940"><rect width="1440" height="940" fill="#0c1322"/>']
def txt(x,y,text,size=18,color='#edf3ff',weight=400):s.append(f'<text x="{x}" y="{y}" font-family="DejaVu Sans,sans-serif" font-size="{size}" font-weight="{weight}" fill="{color}">{html.escape(text)}</text>')
txt(64,78,'MockLite / C# source generation',22,weight=700)
txt(64,146,'FROM APPLICATION CONTRACT TO TEST SCENARIO',13,'#7ce6c5',700)
txt(64,212,'Your next testing API',49,weight=700);txt(64,270,'is already in your interfaces.',49,weight=700)
txt(64,317,'Generate the test double. Discover its members. Describe the behavior that matters.',19,'#b3bfd5')
for x,w,title in [(64,500,'01   Select a contract in your test project'),(584,792,'02   Configure and verify with the generated API')]:
 s.append(f'<rect x="{x}" y="352" width="{w}" height="408" rx="14" fill="#111d30" stroke="#30415d"/><path d="M {x} 406 H {x+w}" stroke="#30415d"/>');txt(x+24,385,title,14,'#b3bfd5',600)
code=re.findall(r'<pre class="code"><code>(.*?)</code></pre>',root.joinpath('source-generation.html').read_text(),re.S)
for markup,x in zip(code,[88,608]):
 p=Runs();p.feed(markup);y=444;col=0
 for line in ''.join(data for _,data in p.out).split('\n'):
  s.append(f'<text x="{x}" y="{y}" font-family="monospace" font-size="16" fill="#d7e5f6">{html.escape(line).replace(' ', '&#160;')}</text>'); y+=25
txt(88,663,'Build → MockPriceFeed',20,'#7ce6c5',600);txt(88,696,'Setup and verification members',16,'#b3bfd5');txt(88,721,'generated for your contract.',16,'#b3bfd5')
for x,h,a,b in [(64,'Discover as you type','Member-specific methods appear','in IntelliSense.'),(514,'Keep behavior typed','Callbacks and return values follow','the interface signature.'),(964,'Inspect what runs','Concrete C# implementations are','generated during your build.')]:
 txt(x,810,h,19,weight=600);txt(x,842,a,16,'#aabbd3');txt(x,867,b,16,'#aabbd3')
txt(64,912,'Illustrated code walkthrough · BbQ.MockLite + BbQ.MockLite.Generators · using BbQ.MockLite; omitted for space',12,'#93a5bf')
s.append('</svg>');svg=''.join(s);root.joinpath('source-generation.svg').write_text(svg)
doc=fitz.open(stream=svg.encode(),filetype='svg');doc[0].get_pixmap().save(root/'source-generation.png')
