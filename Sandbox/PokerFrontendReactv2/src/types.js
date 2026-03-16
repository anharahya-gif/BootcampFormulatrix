/**
 * TypeScript-style JSDoc type definitions for Poker API
 * @module types
 */

/**
 * @typedef {'Hearts' | 'Diamonds' | 'Clubs' | 'Spades'} Suit
 */

/**
 * @typedef {'Two' | 'Three' | 'Four' | 'Five' | 'Six' | 'Seven' | 'Eight' | 'Nine' | 'Ten' | 'Jack' | 'Queen' | 'King' | 'Ace'} Rank
 */

/**
 * @typedef {'HighCard' | 'Pair' | 'TwoPair' | 'ThreeOfAKind' | 'Straight' | 'Flush' | 'FullHouse' | 'FourOfAKind' | 'StraightFlush'} HandRank
 */

/**
 * @typedef {'PreFlop' | 'Flop' | 'Turn' | 'River' | 'Showdown'} GamePhase
 */

/**
 * @typedef {'Active' | 'Folded' | 'AllIn' | 'Eliminated'} PlayerState
 */

/**
 * @typedef {'WaitingForPlayers' | 'InProgress' | 'Completed'} GameState
 */

/**
 * @typedef {Object} PlayerPublicStateDto
 * @property {string} name - Player name
 * @property {number} chipStack - Current chip count
 * @property {number} currentBet - Current bet in this round
 * @property {boolean} isFolded - Whether player has folded
 * @property {number} seatIndex - Seat position (0-9), -1 if not seated
 * @property {PlayerState} state - Player state
 * @property {string[]} hand - Player's cards (e.g., ["Ace of Spades", "King of Hearts"])
 */

/**
 * @typedef {Object} ShowdownDto
 * @property {string[]} winners - Names of winning players
 * @property {HandRank} handRank - Winning hand rank
 * @property {string} message - Description message
 */

/**
 * @typedef {Object} GameStateDto
 * @property {GameState} gameState - Current game state
 * @property {GamePhase} phase - Current game phase
 * @property {string|null} currentPlayer - Name of player whose turn it is
 * @property {number} currentBet - Current bet to match
 * @property {number} pot - Total pot amount
 * @property {string[]} communityCards - Community cards (e.g., ["Ace of Spades"])
 * @property {PlayerPublicStateDto[]} players - All players
 * @property {ShowdownDto|null} showdown - Showdown details if in showdown phase
 */

/**
 * @typedef {Object} ServiceResult
 * @property {boolean} isSuccess - Whether the operation succeeded
 * @property {string} message - Result message
 */

/**
 * @typedef {Object} AddChipsRequest
 * @property {string} PlayerName - Player name (PascalCase for backend)
 * @property {number} Amount - Chip amount to add
 */

export { };
