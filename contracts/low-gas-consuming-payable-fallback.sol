pragma solidity ^0.4.22;

// 0xdBe3A455Ae330645D931817B7440b1C4f6DcF549, uses 21480 of gas
contract LowGasConsumingPayableFallback {

    function() public payable { 

        for (uint i = 0; i < 10; i++) {}

    }
}