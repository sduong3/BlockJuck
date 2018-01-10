using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XnaCards;
using System;

namespace Blockjuck361
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // max valid blockjuck score for a hand
        const int MaxHandValue = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TopCardOffset = 100;
        const int HorizontalCardOffset = 150;
        const int VerticalCardSpacing = 125;

        // messages
        SpriteFont messageFont;
        const string ScoreMessagePrefix = "Score: ";
        string outcome;
        Message playerScoreMessage;
        Message dealerScoreMessage;
        Message winnerMessage;
		List<Message> messages = new List<Message>();

        // message placement
        const int ScoreMessageTopOffset = 25;
        const int HorizontalMessageOffset = HorizontalCardOffset;
        Vector2 winnerMessageLocation = new Vector2(WindowWidth / 2, WindowHeight / 2);
        Vector2 playerScoreLocation;
        Vector2 dealerScoreLocation;

        // menu buttons
        Texture2D quitButtonSprite;
        Texture2D hitButtonSprite;
        Texture2D standButtonSprite;

        List<MenuButton> menuButtons = new List<MenuButton>();
        MenuButton quitButton;

        // menu button placement
        const int TopMenuButtonOffset = TopCardOffset;
        const int QuitMenuButtonOffset = WindowHeight - TopCardOffset;
        const int HorizontalMenuButtonOffset = WindowWidth / 2;
        const int VeryicalMenuButtonSpacing = 125;
        Vector2 topButtonVector, bottomButtonVector, quitButtonVector;
        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;
        int playerHitCount;
        int dealerHitCount;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = WindowWidth;
            graphics.PreferredBackBufferHeight = WindowHeight;

            // set resolution and show mouse
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            playerHitCount = 2;
            dealerHitCount = 2;

            // create and shuffle deck
            deck = new Deck(Content, 50, 50);
            deck.Shuffle();

            // first player card
            Card playerCard1 = deck.TakeTopCard();
            playerCard1.X = HorizontalCardOffset;
            playerCard1.Y = TopCardOffset;
            playerCard1.FlipOver();
            playerHand.Add(playerCard1);

            // first dealer card. Face Down
            Card dealerCard1 = deck.TakeTopCard();
            dealerCard1.X = WindowWidth - HorizontalCardOffset;
            dealerCard1.Y = TopCardOffset;
            dealerHand.Add(dealerCard1);

            // second player card
            Card playerCard2 = deck.TakeTopCard();
            playerCard2.X = HorizontalCardOffset;
            playerCard2.Y = TopCardOffset + VerticalCardSpacing;
            playerCard2.FlipOver();
            playerHand.Add(playerCard2);

            // second dealer card
            Card dealerCard2 = deck.TakeTopCard();
            dealerCard2.X = WindowWidth - HorizontalCardOffset;
            dealerCard2.Y = TopCardOffset + VerticalCardSpacing;
            dealerCard2.FlipOver();
            dealerHand.Add(dealerCard2);

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>(@"fonts\Arial24");
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(),
                messageFont,
                new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));
            messages.Add(playerScoreMessage);

            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(),
                    messageFont,
                    new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset));

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>(@"graphics\quitbutton");
            hitButtonSprite = Content.Load<Texture2D>(@"graphics\hitbutton");
            standButtonSprite = Content.Load<Texture2D>(@"graphics\standbutton");

            quitButtonVector = new Vector2(HorizontalMenuButtonOffset, QuitMenuButtonOffset);
            quitButton = new MenuButton(quitButtonSprite, quitButtonVector, GameState.Exiting);

            // create hit button and add to list
            topButtonVector = new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset);
            menuButtons.Add(new MenuButton(hitButtonSprite, topButtonVector, GameState.PlayerHitting));

            // create stand button and add to list
            bottomButtonVector = new Vector2(HorizontalMenuButtonOffset, TopMenuButtonOffset + VeryicalMenuButtonSpacing);
            menuButtons.Add(new MenuButton(standButtonSprite, bottomButtonVector, GameState.WaitingForDealer));

            outcome = "temp";
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // update menu buttons as appropriate
            menuButtons.ForEach(b => b.Update(Mouse.GetState()));
            quitButton.Update(Mouse.GetState());


            // game state-specific processing
            switch (currentState)
            {
                case GameState.WaitingForPlayer:
                    break;

                case GameState.PlayerHitting:
                    Card temp = deck.TakeTopCard();
                    temp.X = HorizontalCardOffset;
                    temp.Y = TopCardOffset + VerticalCardSpacing * playerHitCount++;
                    temp.FlipOver();
                    playerHand.Add(temp);
                    playerHit = true;
                    currentState = GameState.WaitingForDealer;
                    break;

                case GameState.WaitingForDealer:
                    currentState = (GetBlockjuckScore(dealerHand) <= 16) ? GameState.DealerHitting : GameState.CheckingHandOver;
                    break;

                case GameState.DealerHitting:
                    if (GetBlockjuckScore(dealerHand) <= 16)
                    {
                        Card temp2 = deck.TakeTopCard();
                        temp2.X = WindowWidth - HorizontalCardOffset;
                        temp2.Y = TopCardOffset + VerticalCardSpacing * dealerHitCount++;
                        temp2.FlipOver();
                        dealerHand.Add(temp2);
                        dealerHit = true;
                    }
                    else
                        dealerHit = false;

                    currentState = GameState.CheckingHandOver;
                    break;

                case GameState.CheckingHandOver:
                    if ((playerHit == false && dealerHit == false) || 
                            (GetBlockjuckScore(playerHand) > 21 || GetBlockjuckScore(dealerHand) > 21))
                    {
                        dealerHand[0].FlipOver();
                        messages.Add(dealerScoreMessage);
                        menuButtons.Clear();
                        menuButtons.Add(quitButton);
                        currentState = GameState.DisplayingHandResults;
                    }
                    else //if player and dealer both stand
                    {
                        playerHit = false;
                        dealerHit = false;
                        currentState = GameState.WaitingForPlayer;

                    }
                    break;

                case GameState.DisplayingHandResults:
                    int playerScore = GetBlockjuckScore(playerHand);
                    int dealerScore = GetBlockjuckScore(dealerHand);
                    if (((playerScore > MaxHandValue) && (dealerScore > MaxHandValue)) ||
                        (playerScore == dealerScore))
                    {
                        outcome = "It's a Draw";
                    }
                    else if ((playerScore > dealerScore) && (playerScore <= MaxHandValue) || (dealerScore > MaxHandValue))
                    {
                        outcome = "Player Won!";
                    }
                    else if ((dealerScore > playerScore) && (dealerScore <= MaxHandValue) || (playerScore > MaxHandValue))
                    {
                        outcome = "Dealer Won!";
                    }
                    break;

                case GameState.Exiting:
                    Exit();
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);
						
            spriteBatch.Begin();

            // draw hands
            playerHand.ForEach(p => p.Draw(spriteBatch));
            dealerHand.ForEach(d => d.Draw(spriteBatch));

            // draw messages
            playerScoreLocation = new Vector2(HorizontalMessageOffset, ScoreMessageTopOffset);
            playerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(playerHand).ToString(), messageFont, playerScoreLocation);
            playerScoreMessage.Draw(spriteBatch);

            dealerScoreLocation = new Vector2(WindowWidth - HorizontalMessageOffset, ScoreMessageTopOffset);
            dealerScoreMessage = new Message(ScoreMessagePrefix + GetBlockjuckScore(dealerHand).ToString(), messageFont, dealerScoreLocation);
            winnerMessage = new Message(outcome, messageFont, winnerMessageLocation);

            if (currentState == GameState.DisplayingHandResults)
            {
                dealerScoreMessage.Draw(spriteBatch);
                winnerMessage.Draw(spriteBatch);
            }

            // draw menu buttons
            drawButtons();
            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blockjuck score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blockjuck score for the hand</returns>
        private int GetBlockjuckScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlockjuckCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MaxHandValue)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blockjuck value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blockjuck value for the card</returns>
        private int GetBlockjuckCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }
        
        /// <summary>
        /// Draws the menu buttons
        /// </summary>
        public void drawButtons()
        {
            foreach (MenuButton menuButton in menuButtons)
            {
                menuButton.Draw(spriteBatch);
            }
        }
    }
}
