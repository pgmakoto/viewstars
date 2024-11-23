------------------------------------------
 Astro Commons
 Tycho Catalogue Lite Decoder Version 1.0
 http://astronomy.webcrow.jp/tyc/
------------------------------------------

■ 概要
  ティコ星表に集録されている恒星の赤道座標(J2000.0)と視等級をCSVファイルに出力します。

■ 動作環境
  符号化ファイルから復号されるCSVファイルの総データサイズは23.6MB(24,775,297バイト)です。
  符号化ファイルの総データサイズは6.03MB(6,330,690バイト)のため、最低でも29.63MB(23.6MB+6.03MB)が必要です。
  本ソフトを起動するためには、Javaの実行環境が必要です。
  http://www.java.com/ja/download/

■ インストール
  圧縮ファイルを任意のフォルダに解凍して完了です。

■ アンインストール
  レジストリは使用していません。フォルダごと削除してください。

■ ファイル一覧
  -89.dat - +89.dat … 符号化ファイル
  tyc-lite-decoder.jar … デコーダ
  readme.txt … 本ドキュメント

■ 使い方
  1. デコーダ「tyc-lite-decoder.jar」が符号化ファイル「-89.dat」～「+89.dat」と同じ階層のディレクトリに配置されていることを確認します。
  2. デコーダ「tyc-lite-decoder.jar」をダブルクリックします。
  3. CSVファイル「tyc+.csv」と「tyc-.csv」が出力されるまで待機します。

■ 免責
  本ソフトを使用して生じた損害等について、作者は責任は負いません。
  使用者の責任で本ソフトを使用してください。

■ サポート
  バグや要望があれば、下記のメールアドレスにお願いします。
  lab.stardust@gmail.com

■ 出力されるCSVファイルのフォーマット
  赤経(°),赤緯(°),視等級(mag+2)
  例 : 001.23193,00.21963,13 -> 赤経：1.23193°,赤緯：0.21963°,11等級

■ 参考文献
  □ 恒星データ
  VizieR Service
  The Hipparcos and Tycho Catalogues (ESA 1997)
  The main part of Tycho Catalogue[timeSerie] (1058332 rows)
  http://vizier.u-strasbg.fr/viz-bin/VizieR-3?-source=I/239/tyc_main

■ 更新履歴
  □ 2015/04/21 ver 1.0
    ・初回リリース