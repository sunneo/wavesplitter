* Wave Splitter 

This tool aims at providing simple way to split single sound wave file into several sound wave files.

Basically, the input text file is in a format as following:
===============================================================================
cut_volume 0.03 ; sound below this volume is so called cut-point
cut_mute 1.10 ; as cut-point continues 1.10 seconds, the next sound louder than cut_volume is considered next sound
mute 1.10  ; fill 1.10 seconds at generated sound file
auto_gen _c ; use _c as surfix for generated sound file
add_head 0 ; insert 0 seconds padding in front of every cut, padding is sampled from original sound file. As playing, this region is represented by faded-in effect.
add_end 0 ; append 0 seconds period padding after every cut, padding is sampled from original sound file. As playing, this region is represented by faded-out effect.
* ; start region
abandon  ; file name 1
abbreviate ; file name 2
abbreviation  ; file name 3 
abdomen  ; ...
abide  ; you can add as much file name as you want.

# ; end of file name region
===============================================================================

* The ';' semi-colon indicates a begin of comment-line, words after this symbol won't take effect on our parser
* cut_volume, cut_mute, mute, auto_gen, add_head,add_end are default arguments in this file
* The '*', '#' star and sharp symbols indicate begin and end of description respectively. 

In addition, in the file name region, before setting file name , additional temporary setting can be added for
overriding default settings. They are $cut_volume_temp, $cut_mute_temp, $mute_temp, $auto_gen_temp, $add_head_temp, 
$add_end_temp. These temp configurations affect only the enclose filename. 

