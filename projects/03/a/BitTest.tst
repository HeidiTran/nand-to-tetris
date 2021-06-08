load Bit.hdl,
output-file BitTest.out,
output-list time%S1.4.1 in%B2.1.2 load%B2.1.2 out%B2.1.2;

/**
 * Run the clock through 5 unit of time. 
 * The ? represents an unknown output since we don't know what the prev output is yet to output
 * 
 * load 1 -  -
 *      0  -- -
 * 	 
 * in	1 -
 * 	    0  ---- 
 * 	 
 * out  1 ?---
 * 	    0     -
 */

set in 1,
set load 1,
tick,
output;

tock,
output;


set in 0,
set load 0,
tick,
output;

tock,
output;

set in 0,
set load 0,
tick,
output;

tock,
output;

set in 0,
set load 1,
tick,
output;

tock,
output;

set in 0,
set load 0,
tick,
output;

tock,
output;