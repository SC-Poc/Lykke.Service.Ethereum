pragma solidity ^0.4.22;

// 0x40c914c3A8Fc4c3ccC1d3682828928515DFa631b, uses 64050 gas
contract ModerateGasConsumingPayableFallback {

    function() public payable { 

        for (uint i = 0; i < 1000; i++) {}

    }
    
}