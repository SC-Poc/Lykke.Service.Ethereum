pragma solidity ^0.4.22;

// 0xe152a70E09661c248A8e4B883CF8288670785795, uses tons of gas
contract VeryHighGasConsumingPayableFallback {

    address[] callers;

    function() public payable { 

        for (uint i = 0; i < 1000000; i++) {
            callers.push(msg.sender);
        }
    }
}