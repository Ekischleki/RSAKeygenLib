# RSAKeygenLib
This software is able to generate RSA Keypairs fully. This includes generating large prime numbers given a byte size (at least 128 bytes are recommended).
## Generating a Keypair
You may use ```GenerateRSAKeypair(int byteSize)``` or ```GenerateRSAKeypair(BigInteger prime1, BigInteger prime2)``` to generate a PublicPrivateKeypair object, which contains two KeyPair objects: The public, and the private key.
## En/Decrypting using a Keypair
You may use the ```<KeyPairObject>.CryptUsingKeypair(BigIntiger number)``` to encrypt or decrypt with your public/private key.
