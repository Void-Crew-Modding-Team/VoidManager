
using System.Collections.Generic;

namespace VoidManager.Chat.Router
{
    /// <summary>
    /// Handles a command instance for use via in-game chat.
    /// </summary>
    public abstract class ChatCommand
    {
        /// <summary>
        /// Command aliases will fail silently if the alias is not unique
        /// </summary>
        /// <returns>An array containing names for the command that can be used by the player</returns>
        public abstract string[] CommandAliases();

        /// <returns>A short description of what the command does</returns>
        public abstract string Description();

        /// <summary>
        /// If the player presses TAB while typing a command, any Arguments returned from this will be eligable to be autocompleted.<br/>
        /// If multiple arguments match the typed text, Autocomplete will display a list of possible arguments.<br/>
        /// Arguments starting with '%' will be displayed in the list, but not autocompleted.<br/>
        /// The exact string "%player_name" will be replaced with a list of the current player names.<br/>
        /// <br/>
        /// Example:<br/>
        /// /command<br/>
        /// /command &lt;yes | no | index&gt;<br/>
        /// /command &lt;player name&gt; [health | O2 | speed]<br/>
        /// /command &lt;find | set&gt; &lt;price&gt; [option1 | option2]<br/>
        /// /command &lt;find | set&gt; &lt;time&gt; [option3]<br/>
        /// <br/>
        /// should be written as<br/>
        /// <br/>
        /// Argument options12 = new Argument( "option1", "option2" );<br/>
        /// Argument option3 = new Argument( "option3" );<br/>
        /// Argument price = new Argument( "price", new List&lt;Argument&gt;() { options12 });<br/>
        /// Argument time = new Argument( "time", new List&lt;Argument&gt;() { option3 });<br/>
        /// return new List&lt;Argument&gt;()<br/>
        /// {<br/>
        /// new Argument(),<br/>
        /// new Argument( "yes", "no", "%index" ),<br/>
        /// new Argument( new string[] { "%player_name" }, new List&lt;Argument&gt;() {new Argument( "health", "O2", "speed" )}),<br/>
        /// new Argument( new string[] { "find", "get" }, new List&lt;Argument&gt;() { price, time });<br/>
        /// }
        /// </summary>
        /// <returns></returns>
        public virtual List<Argument> Arguments()
        {
            return new List<Argument>();
        }

        /// <returns>Examples of how to use the command including what arguments are valid</returns>
        public virtual string[] UsageExamples()
        {
            return new string[] { $"/{CommandAliases()[0]}" };
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">A string containing all of the text entered after the command</param>
        public abstract void Execute(string arguments);
    }

    /// <summary>
    /// Handles a command instance for use via in-game chat.
    /// </summary>
    public abstract class PublicCommand
    {
        /// <summary>
        /// Command aliases will fail silently if the alias is not unique
        /// </summary>
        /// <returns>An array containing names for the command that can be used by the player</returns>
        public abstract string[] CommandAliases();

        /// <returns>A short description of what the command does</returns>
        public abstract string Description();

        /// <summary>
        /// If the player presses TAB while typing a command, any Arguments returned from this will be eligable to be autocompleted.<br/>
        /// If multiple arguments match the typed text, Autocomplete will display a list of possible arguments.<br/>
        /// Arguments starting with '%' will be displayed in the list, but not autocompleted.<br/>
        /// The exact string "%player_name" will be replaced with a list of the current player names.<br/>
        /// <br/>
        /// Example:<br/>
        /// !command<br/>
        /// !command &lt;yes | no | index&gt;<br/>
        /// !command &lt;player name&gt; [health | O2 | speed]<br/>
        /// !command &lt;find | set&gt; &lt;price&gt; [option1 | option2]<br/>
        /// !command &lt;find | set&gt; &lt;time&gt; [option3]<br/>
        /// <br/>
        /// should be written as<br/>
        /// <br/>
        /// Argument options12 = new Argument( "option1", "option2" );<br/>
        /// Argument option3 = new Argument( "option3" );<br/>
        /// Argument price = new Argument( "price", new List&lt;Argument&gt;() { options12 });<br/>
        /// Argument time = new Argument( "time", new List&lt;Argument&gt;() { option3 });<br/>
        /// return new List&lt;Argument&gt;()<br/>
        /// {<br/>
        /// new Argument(),<br/>
        /// new Argument( "yes", "no", "%index" ),<br/>
        /// new Argument( new string[] { "%player_name" }, new List&lt;Argument&gt;() {new Argument( "health", "O2", "speed" )}),<br/>
        /// new Argument( new string[] { "find", "get" }, new List&lt;Argument&gt;() { price, time });<br/>
        /// }
        /// </summary>
        /// <returns></returns>
        public virtual List<Argument> Arguments()
        {
            return new List<Argument>();
        }

        /// <returns>Examples of how to use the command including what arguments are valid</returns>
        public virtual string[] UsageExamples()
        {
            return new string[] { $"!{CommandAliases()[0]}" };
        }

        /// <summary>
        /// Executes the command
        /// </summary>
        /// <param name="arguments">A string containing all of the text entered after the command</param>
        /// <param name="SenderID">Handler identifier for player</param>
        public abstract void Execute(string arguments, int SenderID);
    }
}
