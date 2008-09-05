src_files = Program.cs Diagram.cs DiagramBuilder.cs Canvas.cs DiagramRenderer.cs

smul.exe: $(src_files)
	gmcs -pkg:gtk-sharp-2.0 -reference:"C:\Program Files\Mono-1.9.1\lib\mono\2.0\Mono.Cairo.dll" $(src_files) -out:smul.exe

