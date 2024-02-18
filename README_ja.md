# plan_checker_gui_esapi_v15_5

プランチェックを行うための ESAPI Binary Plugin です。

患者への照射前に、作成したプランに技術的な問題が無いかを確認します。

ESAPI v15.5 で可能な範囲で作成しています。

<span style="color:#ff0000;">本ソフトウェアの多言語への翻訳は途中です。</span>



## 確認項目

- Virtual couch が Structure set 内に存在するか（VMAT/定位照射用）
- Jaw が MLC の開度よりも設定値の範囲内（[設定方法](##設定方法) 参照）で広く開いているか
- 線量計算アルゴリズムは規定されているものを使用しているか
- 線量率は Field のエネルギーにおける最大値か
- 線量は Isocenter に 100% 処方されているか（コンベ用）
- Isocenter と Reference point の座標が一致しているか（コンベ用）
- Isocenter 座標の小数点第二位が 0 になっているか
- ガントリ角度は整数値になっているか
- MU が 10 未満の Field が無いか
- Jaw 開度が 3 cm 未満の Field が無いか



## 使用方法

確認したいプランを開いた状態で、Tools > Scripts から plan_checker_gui_esapi_v15_5.esapi.dll を実行する。



### 設定方法

[plan_check_parameters.csv](https://github.com/akiaji-k/plan_checker_gui_esapi_v15_5/blob/main/plan_check_parameters.csv) ファイルで確認項目の編集が可能。

編集可能項目を以下に示す。

- Jaw と MLC 開度の差の上限値
- コンベンショナル照射用の線量計算アルゴリズム、Grid size
- IMRT, 定位照射用の線量計算アルゴリズム、Grid size
- 電子線用の線量計算アルゴリズム、Grid size
- リニアックに対応する、エネルギーごとの線量率
- リニアックに対応する Virtual Couch の Comment



各項目は重複して設定することで、複数の値を許容することができる。



## 実行例

以下の実行例には、あえてエラーを作成している。

下図のように警告が発生した場合、右側のウインドウにその情報が表示される。



### コンベンショナル

![plan_check_conv](./images/plan_check_conv.png)



### VMAT

![plan_check_vmat](./images/plan_check_vmat.png)



### 電子線

![plan_check_elec](./images/plan_check_elec.png)



## ライセンス

MIT ライセンスで公開されています。

本ソフトウェアで発生したことについて、いかなる責任も負いません。

詳細は [LICENSE](https://github.com/akiaji-k/plan_checker_gui_esapi_v15_5/blob/main/LICENSE) をご確認ください。

