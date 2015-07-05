using System;

namespace SharpUV
{
	public enum FileAccessMode
	{
		ReadOnly = 0x0000,  /* open for reading only */
		WriteOnly = 0x0001,  /* open for writing only */
		ReadWrite = 0x0002 /* open for reading and writing */
	}

	[Flags]
	public enum FileOpenMode
	{
		Default = 0x0000,
		Append = 0x0008,  /* writes done at eof */
		Create = 0x0100,  /* create and open file */
		Truncate = 0x0200,  /* open and truncate */
		OnlyIfExists = 0x0400,  /* open only if file doesn't already exist */
		TextMode = 0x4000,  /* file mode is text (translated) */
		BinaryMode = 0x8000  /* file mode is binary (untranslated) */
	}

	[Flags]
	public enum FilePermissions
	{
		S_ISUID = 0x0800, // Set user ID on execution
		S_ISGID = 0x0400, // Set group ID on execution
		S_ISVTX = 0x0200, // Save swapped text after use (sticky).
		S_IRUSR = 0x0100, // Read by owner
		S_IWUSR = 0x0080, // Write by owner
		S_IXUSR = 0x0040, // Execute by owner
		S_IRGRP = 0x0020, // Read by group
		S_IWGRP = 0x0010, // Write by group
		S_IXGRP = 0x0008, // Execute by group
		S_IROTH = 0x0004, // Read by other
		S_IWOTH = 0x0002, // Write by other
		S_IXOTH = 0x0001, // Execute by other

		S_IRWXG = (S_IRGRP | S_IWGRP | S_IXGRP),
		S_IRWXU = (S_IRUSR | S_IWUSR | S_IXUSR),
		S_IRWXO = (S_IROTH | S_IWOTH | S_IXOTH),
		ACCESSPERMS = (S_IRWXU | S_IRWXG | S_IRWXO), // 0777
		ALLPERMS = (S_ISUID | S_ISGID | S_ISVTX | S_IRWXU | S_IRWXG | S_IRWXO), // 07777
		DEFFILEMODE = (S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH), // 0666
	}
}
