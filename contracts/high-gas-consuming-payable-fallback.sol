pragma solidity ^0.4.22;

// 0xF9404188b2A53E6a29dA9F960eC318c452E38E3c, uses 150050 of gas
contract HighGasConsumingPayableFallback {

    function() public payable { 

        for (uint i = 0; i < 3000; i++) {}

    }
    
}