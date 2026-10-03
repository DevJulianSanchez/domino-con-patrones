# =====================================================================
# .latexmkrc : XeLaTeX + BibTeX, salida en build/
# Antes de compilar exporta a PDF (vectorial) cada diagramas/*.drawio
# que sea más reciente que su versión en diagramas/exportados/.
# =====================================================================
$pdf_mode = 5;            # XeLaTeX
$bibtex_use = 2;
$out_dir = 'build';
$xelatex = 'xelatex -synctex=1 -interaction=nonstopmode -file-line-error -recorder %O %S';

use File::Basename;
use File::Path qw(make_path);

sub drawio_cli {
  return $ENV{DRAWIO} if $ENV{DRAWIO};
  for my $c ('/Applications/draw.io.app/Contents/MacOS/draw.io',
             'C:/Program Files/draw.io/draw.io.exe') {
    return $c if -x $c;
  }
  for my $dir (split /[:;]/, $ENV{PATH}) {
    return "$dir/drawio" if -x "$dir/drawio";
  }
  return '';
}

sub exportar_diagramas {
  my $cli = drawio_cli();
  make_path('diagramas/exportados');
  for my $src (glob 'diagramas/*.drawio') {
    my $dst = 'diagramas/exportados/' . basename($src, '.drawio') . '.pdf';
    next if -e $dst && (stat $dst)[9] >= (stat $src)[9];
    if (!$cli) {
      warn "latexmkrc: draw.io no encontrado; no se exporta $src (definir DRAWIO)\n";
      next;
    }
    print "latexmkrc: exportando $src -> $dst\n";
    system($cli, '-x', '-f', 'pdf', '--crop', '-o', $dst, $src) == 0
      or warn "latexmkrc: falló la exportación de $src\n";
  }
}

exportar_diagramas();
