pragma solidity ^0.4.22;

// 0x4c9048A2d55765fA051123908e09fcD13492Aa08
contract NonPayableFallbackTest {
    
    address caller;

    function() public {
        caller = msg.sender;
    }
}